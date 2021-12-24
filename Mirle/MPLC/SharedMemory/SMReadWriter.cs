using System.Collections.Generic;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC.SharedMemory
{
    public class SMReadWriter : IMPLCProvider
    {
        protected readonly List<SMDataBlock> _dataBlocks = new List<SMDataBlock>();

        public bool IsConnected => true;

        public SMReadWriter()
        {
        }

        public virtual void AddDataBlock(SMDataBlock newDataBlock)
        {
            _dataBlocks.Add(newDataBlock);
        }

        public virtual bool GetBit(string address)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TryGetBit(address, out bool value))
                {
                    return value;
                }
            }
            return false;
        }

        public virtual void SetBitOn(string address)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TrySetBitOn(address))
                {
                    return;
                }
            }
        }

        public virtual void SetBitOff(string address)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TrySetBitOff(address))
                {
                    return;
                }
            }
        }

        public virtual int ReadWord(string address)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TryGetWord(address, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public virtual void WriteWord(string address, int data)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TrySetWord(address, data))
                {
                    return;
                }
            }
        }

        public virtual int[] ReadWords(string startAddress, int length)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TryGetWords(startAddress, out int[] data, length))
                {
                    return data;
                }
            }
            return new int[length];
        }

        public virtual void WriteWords(string startAddress, int[] data)
        {
            foreach (var block in _dataBlocks)
            {
                if (block.TrySetWords(startAddress, data))
                {
                    return;
                }
            }
        }
    }
}