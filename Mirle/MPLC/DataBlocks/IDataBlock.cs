namespace Mirle.MPLC.DataBlocks
{
    public interface IDataBlock
    {
        void SetRawData(byte[] newRawData);

        byte[] GetRawData();

        bool TryGetBit(string address, out bool value);

        bool TrySetBitOn(string address);

        bool TrySetBitOff(string address);

        bool TryGetWord(string address, out int value);

        bool TrySetWord(string address, int value);

        bool TryGetWords(string address, out int[] data, int length);

        bool TrySetWords(string address, int[] data);
    }
}
