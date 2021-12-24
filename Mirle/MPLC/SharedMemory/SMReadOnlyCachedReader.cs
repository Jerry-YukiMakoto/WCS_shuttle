using System;
using System.Collections.Generic;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC.SharedMemory
{
    public class SMReadOnlyCachedReader : SMReadWriter, IDisposable
    {
        private class CacheBlockMapping
        {
            public IDataBlock SourceBlock { get; set; }
            public IDataBlock CacheBlock { get; set; }
        }

        private readonly List<CacheBlockMapping> _cacheBlockMappings = new List<CacheBlockMapping>();
        private readonly List<DataBlock> _cachedBlocks = new List<DataBlock>();
        private readonly ThreadWorker _cacheWorker;

        public int Interval
        {
            get => _cacheWorker.Interval;
            set => _cacheWorker.Interval = value;
        }

        public SMReadOnlyCachedReader()
        {
            _cacheWorker = new ThreadWorker(CacheProc, 200);
        }

        private void CacheProc()
        {
            foreach (var cacheBlockMapping in _cacheBlockMappings)
            {
                cacheBlockMapping.CacheBlock.SetRawData(cacheBlockMapping.SourceBlock.GetRawData());
            }
        }

        public void Start()
        {
            _cacheWorker.Start();
        }

        public override void AddDataBlock(SMDataBlock newDataBlock)
        {
            _dataBlocks.Add(newDataBlock);
            var cacheBlock = new DataBlock(newDataBlock.DeviceRange);
            _cachedBlocks.Add(cacheBlock);
            _cacheBlockMappings.Add(new CacheBlockMapping()
            {
                SourceBlock = newDataBlock,
                CacheBlock = cacheBlock,
            });
        }

        public override bool GetBit(string address)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetBit(address, out bool value))
                {
                    return value;
                }
            }
            return false;
        }

        public override void SetBitOn(string address)
        {
            return;
        }

        public override void SetBitOff(string address)
        {
            return;
        }

        public override int ReadWord(string address)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetWord(address, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public override void WriteWord(string address, int data)
        {
            return;
        }

        public override int[] ReadWords(string startAddress, int length)
        {
            foreach (var block in _cachedBlocks)
            {
                if (block.TryGetWords(startAddress, out int[] data, length))
                {
                    return data;
                }
            }
            return new int[length];
        }

        public override void WriteWords(string startAddress, int[] data)
        {
            return;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_cacheWorker != null)
                    {
                        _cacheWorker.Pause();
                        _cacheWorker.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~SMReadOnlyCachedReader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}