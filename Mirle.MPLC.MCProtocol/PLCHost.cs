using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Mirle.MPLC.DataBlocks;
using Mirle.MPLC.DataType;

namespace Mirle.MPLC.MCProtocol
{
    public class PLCHost : IDisposable, IMPLCProvider, IPLCHost
    {
        private class RawdataInfo
        {
            public DateTime Datatime { get; set; }
            public string Rawdata { get; set; }
        }

        public bool IsConnected => _mplc != null && _mplc.IsConnected;
        public PLCHostInfo HostInfo { get; }
        public bool EnableWriteShareMemory
        {
            get
            {
                return _enableWriteShareMemory;
            }
            set
            {
                _enableWriteShareMemory = value;
                if (_enableWriteShareMemory)
                {
                    _smBlocks.Clear();
                    foreach (var block in HostInfo.BlockInfos)
                    {
                        _smBlocks.Add(new SMDataBlockInt32(block.DeviceRange, block.SharedMemoryName));
                    }
                }
                else
                {
                    _smBlocks.Clear();
                }
            }
        }
        public bool EnableWriteRawData { get; set; } = false;
        public bool EnableAutoReconnect { get; set; } = true;
        public string LogBaseDirectory { get; set; } = $@"{AppDomain.CurrentDomain.BaseDirectory}LOG";
        public int Interval
        {
            get => _heartbeat.Interval;
            set
            {
                if (value < 200)
                {
                    _heartbeat.Interval = 200;
                }
                else if (value > 10000)
                {
                    _heartbeat.Interval = 10000;
                }
                else
                {
                    _heartbeat.Interval = value;
                }
            }
        }
        public int MPLCTimeout
        {
            get => _mplc?.Timeout ?? 600;
            set
            {
                if (value < 600)
                {
                    _mplc.Timeout = 600;
                }
                else if (value > 60_000)
                {
                    _mplc.Timeout = 60_000;
                }
                else
                {
                    _mplc.Timeout = value;
                }
            }
        }

        private readonly ConcurrentQueue<RawdataInfo> _rawdataQueue = new ConcurrentQueue<RawdataInfo>();
        private readonly List<DataBlock> _cachedBlocks = new List<DataBlock>();
        private readonly List<SMDataBlockInt32> _smBlocks = new List<SMDataBlockInt32>();
        private readonly ReadWriteAdapter _mplc;
        private readonly ThreadWorker _heartbeat;
        private readonly ThreadWorker _writeRawdataWorker;
        private string _lastMPLCWordRecord = string.Empty;
        private bool _enableWriteShareMemory = false;

        public PLCHost(PLCHostInfo plcHostInfo)
        {
            HostInfo = plcHostInfo;

            foreach (var block in HostInfo.BlockInfos)
            {
                _cachedBlocks.Add(new DataBlock(block.DeviceRange));
            }

            _mplc = new ReadWriteAdapter(HostInfo.HostId, HostInfo.IPAddress, HostInfo.TcpPort);
            _mplc.Connect();

            _heartbeat = new ThreadWorker(RunProcess, 200);
            _writeRawdataWorker = new ThreadWorker(WriteRawdataProcess, 1000);
        }

        private void RunProcess()
        {
            try
            {
                ReadPLCDataFormPLC();
                if (EnableWriteShareMemory)
                {
                    WritePLCDataToSharedMemory();
                }

                if (EnableWriteRawData)
                {
                    ExportPLCData();
                }

                if (_mplc.IsConnected == false)
                {
                    TraceLog(_mplc.TestConnectionByPing());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                Task.Delay(1000).Wait();
            }
        }
        private void ReadPLCDataFormPLC()
        {
            if (_mplc.IsConnected)
            {
                foreach (var block in _cachedBlocks)
                {
                    block.SetRawData(_mplc.ReadWords(block.DeviceRange.StartAddress, block.DeviceRange.WordLength).ToBytes());
                }
            }
            else if (EnableAutoReconnect)
            {
                _mplc.ReConnect();
            }
        }
        private void WritePLCDataToSharedMemory()
        {
            for (int i = 0; i < _cachedBlocks.Count; i++)
            {
                _smBlocks[i].SetRawData(_cachedBlocks[i].GetRawData());
            }
        }
        private void ExportPLCData()
        {
            var sb = new StringBuilder();
            foreach (var block in _cachedBlocks)
            {
                sb.Append('|');
                sb.Append(BitConverter.ToString(block.GetRawData()).Replace("-", ""));
            }
            string strTemp = sb.ToString();
            if (_lastMPLCWordRecord != strTemp)
            {
                sb.Append(strTemp);
                _rawdataQueue.Enqueue(new RawdataInfo() { Datatime = DateTime.Now, Rawdata = strTemp });
                _lastMPLCWordRecord = strTemp;
            }
        }

        private void WriteRawdataProcess()
        {
            try
            {
                if (_rawdataQueue.TryPeek(out var rawdataInfo))
                {
                    string logPath = LogBaseDirectory;
                    logPath = logPath.EndsWith(@"\") ? $@"{logPath}{rawdataInfo.Datatime:yyyy-MM-dd}\{HostInfo.HostId}" : $@"{logPath}\{rawdataInfo.Datatime:yyyy-MM-dd}\{HostInfo.HostId}";
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }

                    string lastLogFilename = $@"{logPath}\{HostInfo.HostId}_PLCR_RawData_{rawdataInfo.Datatime:yyyyMMddHH}.log";

                    using (var file = new StreamWriter(File.Open(lastLogFilename, FileMode.Append)))
                    {
                        while (_rawdataQueue.TryPeek(out rawdataInfo))
                        {
                            string logFilename = $@"{logPath}\{HostInfo.HostId}_PLCR_RawData_{rawdataInfo.Datatime:yyyyMMddHH}.log";
                            if (lastLogFilename != logFilename)
                            {
                                break;
                            }

                            file.WriteLine($"[{rawdataInfo.Datatime:HH:mm:ss.fffff}] {rawdataInfo.Rawdata}");
                            _rawdataQueue.TryDequeue(out var result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                if (_rawdataQueue.Count > 1000)
                {
                    _rawdataQueue.TryDequeue(out var result);
                }
            }
        }
        private void TraceLog(string message)
        {
            string logPath = LogBaseDirectory;
            logPath = logPath.EndsWith(@"\") ? $@"{logPath}{DateTime.Now:yyyy-MM-dd}\{HostInfo.HostId}" : $@"{logPath}\{DateTime.Now:yyyy-MM-dd}\{HostInfo.HostId}";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string logFilename = $@"{logPath}\{HostInfo.HostId}_PLCR_Trace.log";

            using (var file = new StreamWriter(File.Open(logFilename, FileMode.Append)))
            {
                file.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
        }

        public bool GetBit(string address)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetBit(address, out bool value))
                {
                    return value;
                }
            }
            return false;
        }

        public void SetBitOn(string address)
        {
            _mplc.SetBitOn(address);
        }

        public void SetBitOff(string address)
        {
            _mplc.SetBitOff(address);
        }

        public int ReadWord(string address)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetWord(address, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public void WriteWord(string address, int data)
        {
            _mplc.WriteWord(address, data);
        }

        public int[] ReadWords(string startAddress, int length)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetWords(startAddress, out int[] data, length))
                {
                    return data;
                }
            }
            return new int[length];
        }

        public void WriteWords(string startAddress, int[] data)
        {
            _mplc.WriteWords(startAddress, data);
        }

        public IMPLCProvider GetMPLCProvider()
        {
            return _mplc;
        }

        public void Stop()
        {
            _heartbeat.Pause();
        }

        public void Start()
        {
            _heartbeat.Start();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _heartbeat.Dispose();
                    _writeRawdataWorker.Dispose();
                    _mplc.Dispose();
                }

                disposedValue = true;
            }
        }

        ~PLCHost()
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