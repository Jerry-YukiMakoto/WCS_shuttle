using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

using NetMQ;
using NetMQ.Sockets;

namespace Mirle.MPLC.MCProtocol
{
    internal class ReadWriteAdapter : IDisposable, IMPLCProvider, IConnectable
    {
        public bool IsConnected { get; private set; } = false;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value < 600)
                {
                    _timeout = 600;
                }
                else if (value > 60_000)
                {
                    _timeout = 60_000;
                }
                else
                {
                    _timeout = value;
                }
            }
        }

        private int _timeout = 5_000;
        private StreamSocket _socket;

        private readonly string _hostId;
        private readonly int _plcPort;
        private readonly IPAddress _plcIPAddress = IPAddress.Parse("127.0.0.1");
        private readonly Frame3E _frame3E;
        private readonly object _plcLock = new object();
        private readonly object _socketLock = new object();

        public ReadWriteAdapter(string hostId, string ipAddress, int port)
        {
            _hostId = hostId;
            _plcPort = port;
            _plcIPAddress = IPAddress.Parse(ipAddress);
            _frame3E = new Frame3E();
        }
        public bool TestConnection()
        {
            return TestConnection(_plcIPAddress.ToString(), _plcPort);
        }
        public bool TestConnection(string ipAddress, int port)
        {
            try
            {
                var client = new TcpClient();
                var ip = IPAddress.Parse(ipAddress);
                var asyncResult = client.BeginConnect(ip, port, null, null);
                bool workIsCompleted = asyncResult.AsyncWaitHandle.WaitOne(_timeout, true);

                if (workIsCompleted && client.Connected)
                {
                    client.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                TraceLog($"TestConnection : {ipAddress}:{port}: {ex}");
                return false;
            }
        }
        internal string TestConnectionByPing()
        {
            using (var ping = new Ping())
            {
                var pingReply = ping.Send(_plcIPAddress);
                return $"TestConnectionByPing : IP: {_plcIPAddress}, {pingReply.Status}";
            }
        }

        public void Close()
        {
            lock (_socketLock)
            {
                IsConnected = false;
                _socket?.Close();
            }
        }

        public bool ReConnect()
        {
            if (IsConnected)
            {
                TraceLog($"ReConnect : Success!");
                return true;
            }
            else
            {
                TraceLog($"ReConnect : Wait NetMQ Reconnect...");
                Close();
                Thread.Sleep(1000);
                return Connect();
            }
        }

        public bool Connect()
        {
            lock (_socketLock)
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        var pingReply = ping.Send(_plcIPAddress);
                        if (pingReply.Status == IPStatus.Success)
                        {
                            _socket?.Close();
                            _socket = new StreamSocket();
                            _socket.Connect($"tcp://{_plcIPAddress}:{_plcPort}");
                            IsConnected = true;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceLog($"IP: {_plcIPAddress}:{_plcPort}: {ex}");
                }

                IsConnected = false;
                _socket?.Close();
                return false;
            }
        }

        public bool GetBit(string address)
        {
            lock (_plcLock)
            {
                if (IsConnected == false)
                {
                    return false;
                }

                var device = MCDevice.Parse(address);
                if (device == null)
                {
                    return false;
                }

                if (device.IsWord)
                {
                    int[] data = RequestReadWords(device, 1);
                    var bitArray = new BitArray(BitConverter.GetBytes(data[0]));
                    return bitArray.Get(device.BitIndex);
                }
                else if (device.IsBit)
                {
                    return RequestReadBit(device);
                }

                return false;
            }
        }

        public void SetBitOn(string address)
        {
            lock (_plcLock)
            {
                SetBit(address, true);
            }
        }
        public void SetBitOff(string address)
        {
            lock (_plcLock)
            {
                SetBit(address, false);
            }
        }

        public int ReadWord(string address)
        {
            lock (_plcLock)
            {
                if (IsConnected == false)
                {
                    return 0;
                }

                var device = MCDevice.Parse(address);
                if (device == null)
                {
                    return 0;
                }

                int[] data = RequestReadWords(device, 1);

                return data?.Length > 0 ? data[0] : 0;
            }
        }
        public int[] ReadWords(string startAddress, int length)
        {
            lock (_plcLock)
            {
                if (IsConnected == false)
                {
                    return new int[0];
                }

                var data = new List<int>();
                int offset = 0;
                do
                {
                    var device = MCDevice.Parse(startAddress, offset);
                    if (device == null)
                    {
                        return null;
                    }

                    int numberOfWords = Math.Min(_frame3E.MaximumWords, length - offset);
                    data.AddRange(RequestReadWords(device, numberOfWords));

                    offset += _frame3E.MaximumWords;
                }
                while (offset < length);

                return data.ToArray();
            }
        }

        public void WriteWord(string address, int data)
        {
            lock (_plcLock)
            {
                if (IsConnected == false)
                {
                    return;
                }

                var device = MCDevice.Parse(address);
                if (device == null)
                {
                    return;
                }

                RequestWriteWords(device, new[] { data });
            }
        }
        public void WriteWords(string startAddress, int[] data)
        {
            lock (_plcLock)
            {
                if (IsConnected == false)
                {
                    return;
                }

                int offset = 0;
                do
                {
                    var device = MCDevice.Parse(startAddress, offset);
                    if (device == null)
                    {
                        return;
                    }

                    int numberOfWords = Math.Min(_frame3E.MaximumWords, data.Length - offset);

                    int[] readyToWriteData = new int[numberOfWords];
                    Array.Copy(data, offset, readyToWriteData, 0, numberOfWords);

                    RequestWriteWords(device, readyToWriteData);

                    offset += _frame3E.MaximumWords;
                }
                while (offset < data.Length);
            }
        }

        private int[] RequestReadWords(MCDevice device, int length)
        {
            try
            {
                byte[] receiveData = SendData(_frame3E.CreateReadWordsFrame(device, length));
                int completeCode = _frame3E.ResolveReadWordsReturnFrame(receiveData, out int[] data);
                if (completeCode != 0)
                {
                    throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + $": MX ErrorCode: 0x{completeCode:X4}");
                }

                return data;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                TraceLog($"MCDevice: {device.AsciiDeviceCode + device.AsciiAddress}.{device.BitIndex}: {ex}");
                return new int[0];
            }
        }
        private bool RequestReadBit(MCDevice device)
        {
            try
            {
                byte[] receiveData = SendData(_frame3E.CreateReadBitFrame(device));
                int completeCode = _frame3E.ResolveReadBitReturnFrame(receiveData, out bool data);
                if (completeCode != 0)
                {
                    throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + $": MX ErrorCode: 0x{completeCode:X4}");
                }

                return data;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                TraceLog($"MCDevice: {device.AsciiDeviceCode + device.AsciiAddress}.{device.BitIndex}: {ex}");
                return false;
            }
        }

        private void SetBit(string address, bool isOn)
        {
            if (IsConnected == false)
            {
                return;
            }

            var device = MCDevice.Parse(address);
            if (device == null)
            {
                return;
            }

            if (device.IsWord)
            {
                int[] data = RequestReadWords(device, 1);

                var bitArray = new BitArray(BitConverter.GetBytes(data[0]));
                bitArray.Set(device.BitIndex, isOn);
                int[] newData = new int[1];
                bitArray.CopyTo(newData, 0);

                RequestWriteWords(device, newData);
            }
            else if (device.IsBit)
            {
                RequestWriteBit(device, isOn);
            }
        }

        private void RequestWriteBit(MCDevice device, bool isOn)
        {
            try
            {
                byte[] receiveData = SendData(_frame3E.CreateWriteBitFrame(device, isOn));
                int completeCode = _frame3E.ResolveWriteBitReturnFrame(receiveData);
                if (completeCode != 0)
                {
                    throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + $": MX ErrorCode: 0x{completeCode:X4}");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                TraceLog($"MCDevice: {device.AsciiDeviceCode + device.AsciiAddress}.{device.BitIndex}: {ex}");
            }
        }

        private void RequestWriteWords(MCDevice device, int[] data)
        {
            try
            {
                byte[] receiveData = SendData(_frame3E.CreateWriteWordsFrame(device, data));
                int completeCode = _frame3E.ResolveWriteWordsReturnFrame(receiveData);
                if (completeCode != 0)
                {
                    throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + $": MX ErrorCode: 0x{completeCode:X4}");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                TraceLog($"MCDevice: {device.AsciiDeviceCode + device.AsciiAddress}.{device.BitIndex}: {ex}");
            }
        }

        private void TraceLog(string message)
        {
            string logPath = $@"{AppDomain.CurrentDomain.BaseDirectory}LOG";
            logPath = logPath.EndsWith(@"\") ? $@"{logPath}{DateTime.Now:yyyy-MM-dd}\{_hostId}" : $@"{logPath}\{DateTime.Now:yyyy-MM-dd}\{_hostId}";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string logFilename = $@"{logPath}\MC_{_plcIPAddress.ToString().Replace('.', '_')}_{_plcPort}_Trace.log";

            using (var file = new StreamWriter(File.Open(logFilename, FileMode.Append)))
            {
                file.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
        }

        private byte[] SendData(byte[] frame)
        {
            lock (_socketLock)
            {
                if (_socket == null || _socket.IsDisposed)
                {
                    IsConnected = false;
                    TraceLog($"SendData: Fail, NoConnect!");
                    return null;
                }

                int retryTimes = 3;
                for (int i = 0; i < retryTimes; i++)
                {
                    try
                    {
                        var timeout = TimeSpan.FromMilliseconds(_timeout / retryTimes);
                        byte[] id = _socket.Options.Identity;
                        if (_socket.TrySendFrame(timeout, id, true)
                           && _socket.TrySendFrame(timeout, frame)
                           && _socket.TryReceiveFrameBytes(timeout, out byte[] receiveId)
                           && _socket.TryReceiveFrameBytes(timeout, out byte[] dataFrame))
                        {
                            IsConnected = true;

                            if (receiveId.SequenceEqual(id))
                            {
                                return dataFrame;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceLog($"SendData: {ex}");
                    }
                }

                IsConnected = false;
                return null;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _socket?.Close();
                    _socket?.Dispose();
                }

                disposedValue = true;
            }
        }

        ~ReadWriteAdapter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}