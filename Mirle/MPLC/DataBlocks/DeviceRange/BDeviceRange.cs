using System;
using System.Diagnostics;
using System.Globalization;

namespace Mirle.MPLC.DataBlocks.DeviceRange
{
    public class BDeviceRange : ITypeDeviceRange
    {
        private const string _type = "B";
        private readonly int _startOffset;
        private readonly int _endOffset;

        public string StartAddress { get; }
        public string EndAddress { get; }
        public int WordLength { get; }
        public int ByteArrayLength => WordLength * 2;

        public BDeviceRange(string startAddress, string endAddress)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
            if (!startAddress.StartsWith(_type) || !endAddress.StartsWith(_type))
            {
                throw new ArgumentException("Wrong Type!!");
            }

            try
            {
                _startOffset = int.Parse(startAddress.Substring(1, startAddress.Length - 1), NumberStyles.HexNumber);
                _endOffset = int.Parse(endAddress.Substring(1, endAddress.Length - 1), NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                throw new ArgumentException("Wrong Address!!");
            }

            if (_startOffset > _endOffset)
            {
                throw new ArgumentException("Wrong Address Range!!");
            }

            int length = _endOffset - _startOffset + 1;

            WordLength = length % 16 == 0 ? length / 16 : (length / 16) + 1;
        }

        public bool IsSameRange(string address)
        {
            return TryGetIndex(address, out int index);
        }

        private static bool IsSameType(string address)
        {
            return address.ToUpper().StartsWith(_type);
        }

        public bool TryGetIndex(string address, out int index)
        {
            if (!IsSameType(address))
            {
                index = -1;
                return false;
            }

            address = address.Substring(1, address.Length - 1);
            address = address.Split('.')[0];
            index = int.Parse(address, NumberStyles.HexNumber);

            if (index >= _startOffset && index <= _endOffset)
            {
                return true;
            }
            index = -1;
            return false;
        }

        public bool TryGetOffset(string address, out int offset)
        {
            if (TryGetIndex(address, out int index))
            {
                offset = index - _startOffset;
                return true;
            }
            offset = -1;
            return false;
        }

        public bool TryGetByteArrayOffset(string address, out int offset)
        {
            if (TryGetIndex(address, out int index))
            {
                offset = (index - _startOffset) / 16 * 2;
                return true;
            }
            offset = -1;
            return false;
        }

        public bool TryGetByteArrayBitIndex(string address, out int index)
        {
            if (TryGetIndex(address, out int bitIndex))
            {
                index = (bitIndex - _startOffset) % 16;
                return true;
            }
            index = -1;
            return false;
        }
    }
}