namespace Mirle.MPLC
{
    public interface IMPLCProvider
    {
        bool IsConnected { get; }

        bool GetBit(string address);

        void SetBitOn(string address);

        void SetBitOff(string address);

        int ReadWord(string address);

        void WriteWord(string address, int data);

        int[] ReadWords(string startAddress, int length);

        void WriteWords(string startAddress, int[] data);
    }
}
