namespace Mirle.MPLC
{
    public class ReadWriteWrapper : IMPLCProvider
    {
        private IMPLCProvider _reader;
        private IMPLCProvider _writer;
        private readonly object _switchReaderLock = new object();
        private readonly object _switchWriterLock = new object();

        public bool IsConnected => _reader.IsConnected && _writer.IsConnected;

        public ReadWriteWrapper(IMPLCProvider reader, IMPLCProvider writer)
        {
            _reader = reader ?? new NullMPLCProvider();
            _writer = writer ?? new NullMPLCProvider();
        }

        public void SetMPLCReader(IMPLCProvider reader)
        {
            lock (_switchReaderLock)
            {
                _reader = reader ?? new NullMPLCProvider();
            }
        }

        public void SetMPLCWriter(IMPLCProvider writer)
        {
            lock (_switchWriterLock)
            {
                _writer = writer ?? new NullMPLCProvider();
            }
        }

        public bool GetBit(string address)
        {
            lock (_switchReaderLock)
            {
                return _reader.GetBit(address);
            }
        }

        public void SetBitOn(string address)
        {
            lock (_switchWriterLock)
            {
                _writer.SetBitOn(address);
            }
        }

        public void SetBitOff(string address)
        {
            lock (_switchWriterLock)
            {
                _writer.SetBitOff(address);
            }
        }

        public int ReadWord(string address)
        {
            lock (_switchReaderLock)
            {
                return _reader.ReadWord(address);
            }
        }

        public void WriteWord(string address, int data)
        {
            lock (_switchWriterLock)
            {
                _writer.WriteWord(address, data);
            }
        }

        public int[] ReadWords(string startAddress, int length)
        {
            lock (_switchReaderLock)
            {
                return _reader.ReadWords(startAddress, length);
            }
        }

        public void WriteWords(string startAddress, int[] data)
        {
            lock (_switchWriterLock)
            {
                _writer.WriteWords(startAddress, data);
            }
        }

        private sealed class NullMPLCProvider : IMPLCProvider
        {
            public bool IsConnected { get; } = false;

            public bool GetBit(string address)
            {
                return false;
            }

            public void SetBitOn(string address)
            {
                return;
            }

            public void SetBitOff(string address)
            {
                return;
            }

            public int ReadWord(string address)
            {
                return 0;
            }

            public void WriteWord(string address, int data)
            {
                return;
            }

            public int[] ReadWords(string startAddress, int length)
            {
                return new int[length];
            }

            public void WriteWords(string startAddress, int[] data)
            {
                return;
            }
        }
    }
}
