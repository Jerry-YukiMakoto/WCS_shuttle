namespace Mirle.MPLC.DataType
{
    public sealed class Word : IDataType
    {
        private readonly IMPLCProvider _mplc;

        public string Address { get; }

        public Word()
        {
        }
        public Word(IMPLCProvider mplc, string address)
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
                return _mplc.ReadWord(Address);
            }
        }

        public void SetValue(int data)
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
            }
            else
            {
                _mplc.WriteWord(Address, data);
            }
        }

        public void Clear()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
            }
            else
            {
                _mplc.WriteWord(Address, 0);
            }
        }

        public Bit GetBit(int bitIndex)
        {
            if (bitIndex < 0 && bitIndex > 15)
            {
                return new Bit();
            }
            else
            {
                return new Bit(_mplc, $"{Address}.{bitIndex:X}");
            }
        }
    }
}