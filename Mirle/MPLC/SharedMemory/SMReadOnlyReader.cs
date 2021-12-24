namespace Mirle.MPLC.SharedMemory
{
    public class SMReadOnlyReader : SMReadWriter
    {
        public override void SetBitOn(string address)
        {
            return;
        }

        public override void SetBitOff(string address)
        {
            return;
        }

        public override void WriteWord(string address, int data)
        {
            return;
        }

        public override void WriteWords(string startAddress, int[] data)
        {
            return;
        }
    }
}