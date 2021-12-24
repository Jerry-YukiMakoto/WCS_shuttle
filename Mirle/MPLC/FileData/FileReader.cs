using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC.FileData
{
    public class FileReader : IMPLCProvider, IDisposable
    {
        private readonly List<string> files = new List<string>();
        private Task _createCacheTask;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<FileDataBlock> _dataBlocks = new List<FileDataBlock>();

        protected ConcurrentDictionary<DateTime, RawRecord> _rawData = new ConcurrentDictionary<DateTime, RawRecord>();

        public double CachingPercentage => (double)_rawData.Values.Count(r => r.IsCached) / _rawData.Count * 100;
        public DateTime CurrentRowTime { get; private set; }

        public FileReader()
        {
        }

        public void AddDataBlock(FileDataBlock newDataBlock)
        {
            _dataBlocks.Add(newDataBlock);
        }

        public IEnumerable<FileDataBlock> GetDataBlocks()
        {
            return _dataBlocks;
        }

        public void AddFile(string FileName)
        {
            WaitForCreateCacheTask();

            files.Add(FileName);
        }

        public void ClearFile()
        {
            WaitForCreateCacheTask();

            _rawData.Clear();
            files.Clear();
        }

        public void OpenFile()
        {
            WaitForCreateCacheTask();

            foreach (string f in files)
            {
                try
                {
                    string[] rows = System.IO.File.ReadAllLines(f);

                    foreach (string row in rows)
                    {
                        var time = DateTime.ParseExact(row.Substring(row.IndexOf('[') + 1, 14), "HH:mm:ss.fffff", null);
                        var rawRecord = CreateRawRecord(row.Substring(row.IndexOf(']') + 2));
                        _rawData.TryAdd(time, rawRecord);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}-{ex.StackTrace}");
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _createCacheTask = new Task(() => CreateCache(_cancellationTokenSource.Token));
            _createCacheTask.Start();
        }

        private void WaitForCreateCacheTask()
        {
            if (_createCacheTask != null && _createCacheTask.Status == TaskStatus.Running)
            {
                _cancellationTokenSource.Cancel();
                _createCacheTask.Wait(1000);
            }
        }

        private void CreateCache(CancellationToken token)
        {
            try
            {
                var po = new ParallelOptions();
                po.CancellationToken = token;
                po.MaxDegreeOfParallelism = Environment.ProcessorCount;

                Parallel.ForEach(_rawData.Values, po, (r) =>
                {
                    r.CreateCache();
                });
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
        }

        protected virtual RawRecord CreateRawRecord(string rawString)
        {
            return new RawRecord(rawString);
        }

        public IEnumerable<DateTime> GetDateTimeIndexes()
        {
            return _rawData.Keys.OrderBy(k => k);
        }

        public void Refresh(int index)
        {
            try
            {
                foreach (var block in _dataBlocks)
                {
                    try
                    {
                        var rawRecords = _rawData.OrderBy(r => r.Key).ToList();
                        var rawRecord = rawRecords[index];
                        CurrentRowTime = rawRecord.Key;
                        byte[] newByteArray = rawRecord.Value.GetBlockByIndex(block.ColumnIndex);
                        block.SetRawData(newByteArray);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex.Message}-{ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
        }

        public void Refresh(DateTime index)
        {
            if (_rawData.TryGetValue(index, out var value))
            {
                try
                {
                    foreach (var block in _dataBlocks)
                    {
                        try
                        {
                            byte[] newByteArray = value.GetBlockByIndex(block.ColumnIndex);
                            block.SetRawData(newByteArray);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{ex.Message}-{ex.StackTrace}");
                        }
                    }

                    CurrentRowTime = index;
                }
                catch (Exception ex)
                { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            }
        }

        public bool IsConnected => true;

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

        public FileDataViewer GetDataView()
        {
            return new FileDataViewer(this);
        }

        public byte[] GetRawDataByDateTimeIndex(DateTime dateTimeIndex, int blockColumnIndex)
        {
            if (_rawData.TryGetValue(dateTimeIndex, out var value))
            {
                return value.GetBlockByIndex(blockColumnIndex);
            }

            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_createCacheTask != null)
                    {
                        _createCacheTask.Dispose();
                    }
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~FileReader()
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