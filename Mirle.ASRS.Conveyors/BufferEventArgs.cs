using System;

namespace Mirle.ASRS.Conveyors
{
    public class BufferEventArgs : EventArgs
    {
        public int BufferIndex { get; }
        public string BufferName { get; }

        public BufferEventArgs(int bufferIndex, string bufferName)
        {
            BufferIndex = bufferIndex;
            BufferName = bufferName;
        }
    }
}
