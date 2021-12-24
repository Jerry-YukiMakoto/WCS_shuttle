namespace Mirle.MPLC.DataType
{
    public sealed class Bit : IDataType
    {
        private readonly IMPLCProvider _mplc;

        public string Address { get; }

        public Bit()
        {
        }
        public Bit(IMPLCProvider mplc, string address)
        {
            _mplc = mplc;
            Address = address;
        }

        public bool IsOn()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return false;
            }
            return _mplc.GetBit(Address);
        }

        public bool IsOff()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return false;
            }
            return !_mplc.GetBit(Address);
        }

        public void SetOn()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return;
            }
            _mplc.SetBitOn(Address);
        }

        public void SetOff()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return;
            }
            _mplc.SetBitOff(Address);
        }

        public void Clear()
        {
            if (_mplc is null || string.IsNullOrWhiteSpace(Address))
            {
                return;
            }
            else
            {
                _mplc.SetBitOff(Address);
            }
        }
    }
}