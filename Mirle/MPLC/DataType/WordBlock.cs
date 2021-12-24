using System;

namespace Mirle.MPLC.DataType
{
    public sealed class WordBlock : IDataType
    {
        private readonly IMPLCProvider _mplc;

        public int Length { get; } = 0;
        public string Address { get; }

        public WordBlock()
        {
        }
        public WordBlock(IMPLCProvider mplc, string startAddress, int length)
        {
            _mplc = mplc;
            Address = startAddress;
            Length = length >= 1 ? length : 1;
        }

        public int[] GetValue()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return new int[0];
            }
            else
            {
                return _mplc.ReadWords(Address, Length);
            }
        }

        public void SetValue(int[] data)
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
            }
            else
            {
                int[] rawData = new int[Length];
                Array.Copy(data, 0, rawData, 0, Math.Min(data.Length, rawData.Length));
                _mplc.WriteWords(Address, rawData);
            }
        }

        public void Clear()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
            }
            else
            {
                int[] eraseData = new int[Length];
                _mplc.WriteWords(Address, eraseData);
            }
        }
    }
}