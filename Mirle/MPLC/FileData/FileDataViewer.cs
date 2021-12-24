using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC.FileData
{
    public class FileDataViewer : IMPLCProvider
    {
        private readonly FileReader _fileReader;
        private readonly List<FileDataBlock> _dataBlocks = new List<FileDataBlock>();

        public bool IsConnected => true;

        public FileDataViewer(FileReader fileReader)
        {
            _fileReader = fileReader;
            foreach (var fileReaderDataBlock in _fileReader.GetDataBlocks())
            {
                _dataBlocks.Add(new FileDataBlock(fileReaderDataBlock.DeviceRange, fileReaderDataBlock.ColumnIndex));
            }
        }

        public IEnumerable<DateTime> Query(DateTime begin, DateTime end)
        {
            return _fileReader.GetDateTimeIndexes().Where(t => t.TimeOfDay >= begin.TimeOfDay && t.TimeOfDay <= end.TimeOfDay).OrderBy(i => i);
        }

        public void RefreshRawData(DateTime index)
        {
            foreach (var block in _dataBlocks)
            {
                try
                {
                    byte[] newByteArray = _fileReader.GetRawDataByDateTimeIndex(index, block.ColumnIndex);
                    if (newByteArray != null)
                    {
                        block.SetRawData(newByteArray);
                    }
                }
                catch (Exception ex)
                { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            }
        }

        public bool GetBit(string address)
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
            foreach (var block in _dataBlocks)
            {
                if (block.TryGetWord(address, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public void WriteWord(string address, int data)
        {
            return;
        }

        public int[] ReadWords(string startAddress, int length)
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

        public void WriteWords(string startAddress, int[] data)
        {
            return;
        }
    }
}