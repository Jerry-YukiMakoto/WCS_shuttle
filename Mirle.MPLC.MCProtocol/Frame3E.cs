using System;
using System.Collections.Generic;

namespace Mirle.MPLC.MCProtocol
{
    internal class Frame3E
    {
        private struct CompleteCode
        {
            public const int NoData = 0xF000;
            public const int InvalidData = 0xF001;
        }

        private const byte _PCNo = 0xff;
        private const byte _IONo_L = 0xff;//0x03ff
        private const byte _IONo_H = 0x03;//0x03ff
        private const byte _CPUTimer_L = 0x10;//0x0010
        private const byte _CPUTimer_H = 0x00;
        private const int _receiceDataMinimumBytes = 11;
        private readonly byte _networkNo = 0x00;
        private readonly byte _stationNo = 0x00;

        public readonly int MaximumWords = 960;

        public Frame3E(int networkNo = 0, int stationNo = 0)
        {
            _networkNo = (byte)networkNo;
            _stationNo = (byte)stationNo;
        }

        private byte[] CreateFrame(int mainCmd, int subCmd, int address, byte deviceCode, int size, IReadOnlyCollection<byte> data)
        {
            int dataLength = 12 + data.Count;

            var frame = new List<byte>()
            {
                0x50, 0x00, //3E Frame
                _networkNo,
                _PCNo,
                _IONo_L,
                _IONo_H,
                _stationNo,
                (byte)(dataLength % 256),
                (byte)(dataLength / 256),
                _CPUTimer_L , _CPUTimer_H,
                (byte)mainCmd, (byte)(mainCmd >> 8),
                (byte)subCmd, (byte)(subCmd >> 8),
                (byte)address, (byte)(address >> 8),
                (byte)(address >> 16),
                deviceCode,
                (byte)size,
                (byte)(size >> 8),
            };

            frame.AddRange(data);

            return frame.ToArray();
        }

        public byte[] CreateReadWordsFrame(MCDevice device, int length)
        {
            if (device == null && length < 1)
            {
                return null;
            }

            if (length > MaximumWords)
            {
                length = MaximumWords;
            }

            int mainCmd = 0x0401;
            int subCmd = 0x0000;
            int address = device.Address;
            byte deviceCode = device.BinaryDeviceCode;
            int size = length;

            return CreateFrame(mainCmd, subCmd, address, deviceCode, size, new List<byte>());
        }

        public byte[] CreateWriteWordsFrame(MCDevice device, int[] data)
        {
            if (device == null && data == null)
            {
                return null;
            }

            int mainCmd = 0x1401;
            int subCmd = 0x0000;
            int address = device.Address;
            byte deviceCode = device.BinaryDeviceCode;
            int size = Math.Min(data.Length, MaximumWords);

            var byteData = new List<byte>();
            for (int i = 0; i < Math.Min(data.Length, MaximumWords); i++)
            {
                byteData.Add((byte)data[i]);
                byteData.Add((byte)(data[i] >> 8));
            }

            return CreateFrame(mainCmd, subCmd, address, deviceCode, size, byteData);
        }

        public byte[] CreateReadBitFrame(MCDevice device)
        {
            if (device == null)
            {
                return null;
            }

            int mainCmd = 0x0401;
            int subCmd = 0x0001;
            int address = device.Address;
            byte deviceCode = device.BinaryDeviceCode;
            int size = 1;

            return CreateFrame(mainCmd, subCmd, address, deviceCode, size, new List<byte>());
        }

        public byte[] CreateWriteBitFrame(MCDevice device, bool isOn)
        {
            if (device == null)
            {
                return null;
            }

            int mainCmd = 0x1401;
            int subCmd = 0x0001;
            int address = device.Address;
            byte deviceCode = device.BinaryDeviceCode;
            int size = 1;
            var byteData = new List<byte>();
            byteData.Add((byte)(isOn ? 0x10 : 0x00));

            return CreateFrame(mainCmd, subCmd, address, deviceCode, size, byteData);
        }

        public int ResolveReadBitReturnFrame(byte[] receiveData, out bool value)
        {
            if (receiveData == null || receiveData.Length < _receiceDataMinimumBytes)
            {
                value = false;
                return CompleteCode.NoData;
            }

            ushort dataBytesLength = BitConverter.ToUInt16(receiveData, 7); //+ CompleteCode 2Bytes
            ushort completeCode = BitConverter.ToUInt16(receiveData, 9);

            if (receiveData.Length != 9 + dataBytesLength)
            {
                value = false;
                return CompleteCode.InvalidData;
            }

            value = receiveData[11] == 0x10;

            return completeCode;
        }

        public int ResolveWriteBitReturnFrame(byte[] receiveData)
        {
            if (receiveData == null || receiveData.Length < _receiceDataMinimumBytes)
            {
                return CompleteCode.NoData;
            }

            ushort dataBytesLength = BitConverter.ToUInt16(receiveData, 7); //+ CompleteCode 2Bytes
            ushort completeCode = BitConverter.ToUInt16(receiveData, 9);

            if (receiveData.Length != 9 + dataBytesLength)
            {
                return CompleteCode.InvalidData;
            }

            return completeCode;
        }

        public int ResolveReadWordsReturnFrame(byte[] receiveData, out int[] data)
        {
            if (receiveData == null || receiveData.Length < _receiceDataMinimumBytes)
            {
                data = null;
                return CompleteCode.NoData;
            }

            ushort dataBytesLength = BitConverter.ToUInt16(receiveData, 7); //+ CompleteCode 2Bytes
            ushort completeCode = BitConverter.ToUInt16(receiveData, 9);

            if (receiveData.Length != 9 + dataBytesLength)
            {
                data = null;
                return CompleteCode.InvalidData;
            }

            data = new int[(dataBytesLength - 2) / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt16(receiveData, _receiceDataMinimumBytes + (i * 2));
            }

            return completeCode;
        }

        public int ResolveWriteWordsReturnFrame(byte[] receiveData)
        {
            if (receiveData == null || receiveData.Length < _receiceDataMinimumBytes)
            {
                return CompleteCode.NoData;
            }

            ushort dataBytesLength = BitConverter.ToUInt16(receiveData, 7); //+ CompleteCode 2Bytes
            ushort completeCode = BitConverter.ToUInt16(receiveData, 9);

            if (receiveData.Length != 9 + dataBytesLength)
            {
                return CompleteCode.InvalidData;
            }

            return completeCode;
        }
    }
}