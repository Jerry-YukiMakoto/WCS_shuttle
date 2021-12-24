using System;

namespace Mirle.MPLC.DataType
{
    public sealed class DWord : IDataType
    {
        private readonly IMPLCProvider _mplc;

        public string Address { get; }

        public DWord()
        {
        }
        public DWord(IMPLCProvider mplc, string address)
        {
            _mplc = mplc;
            Address = address;
        }

        public int GetValue()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return 0;
            }
            else
            {
                int[] data = _mplc.ReadWords(Address, 2);
                return (data[1] * 65536) + data[0];
            }
        }

        public void SetValue(int data)
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
            }
            else
            {
                int[] rawData = new int[2];
                rawData[0] = data % 65536;
                rawData[1] = data / 65536;
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
                _mplc.WriteWords(Address, new int[2]);
            }
        }
    }
}