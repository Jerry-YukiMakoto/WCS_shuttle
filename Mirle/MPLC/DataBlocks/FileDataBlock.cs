using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

using Mirle.MPLC.DataBlocks.DeviceRange;

namespace Mirle.MPLC.DataBlocks
{
    public class FileDataBlock : IDataBlock, IDisposable
    {
        private byte[] _rawData = new byte[1];
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public FileDataBlock(ITypeDeviceRange deviceRange, int columnIndex)
        {
            DeviceRange = deviceRange;
            _rawData = new byte[deviceRange.ByteArrayLength];
            ColumnIndex = columnIndex;
        }
        public ITypeDeviceRange DeviceRange { get; }
        public int ColumnIndex { get; }

        public void SetRawData(byte[] newRawData)
        {
            try
            {
                _rwLock.EnterWriteLock();
                _rawData = newRawData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public byte[] GetRawData()
        {
            byte[] data = new byte[DeviceRange.ByteArrayLength];
            try
            {
                _rwLock.EnterReadLock();
                if (_rawData != null)
                {
                    Array.Copy(_rawData, data, Math.Min(_rawData.Length, data.Length));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return data;
        }

        public bool TryGetBit(string address, out bool value)
        {
            value = false;
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset) && DeviceRange.TryGetByteArrayBitIndex(address, out int index))
                {
                    if (_rawData != null && _rawData.Length > offset)
                    {
                        ushort word = BitConverter.ToUInt16(_rawData, offset);
                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        value = bitArray.Get(index);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        public bool TrySetBitOn(string address)
        {
            return SetBit(address, true);
        }

        private bool SetBit(string address, bool IsOn)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset) && DeviceRange.TryGetByteArrayBitIndex(address, out int index))
                {
                    if (_rawData != null && _rawData.Length > offset + 1)
                    {
                        ushort word = BitConverter.ToUInt16(_rawData, offset);
                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        bitArray.Set(index, IsOn);

                        byte[] tmpBytes = new byte[2];
                        bitArray.CopyTo(tmpBytes, 0);
                        _rawData[offset] = tmpBytes[0];
                        _rawData[offset + 1] = tmpBytes[1];

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            return false;
        }

        public bool TrySetBitOff(string address)
        {
            return SetBit(address, false);
        }

        public bool TryGetWord(string address, out int value)
        {
            value = 0;
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    if (_rawData != null && _rawData.Length > offset)
                    {
                        value = BitConverter.ToUInt16(_rawData, offset);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        public bool TrySetWord(string address, int value)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    if (_rawData != null && _rawData.Length > offset + 1)
                    {
                        byte[] tmpBytes = BitConverter.GetBytes(value);
                        _rawData[offset] = tmpBytes[0];
                        _rawData[offset + 1] = tmpBytes[1];

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return false;
        }

        public bool TryGetWords(string address, out int[] data, int length)
        {
            data = new int[length];
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    if (_rawData != null && _rawData.Length > offset + length)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            data[i] = BitConverter.ToUInt16(_rawData, offset + (i * 2));
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        public bool TrySetWords(string address, int[] data)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    if (_rawData != null && _rawData.Length > offset + data.Length)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            byte[] tmpBytes = BitConverter.GetBytes(data[i]);
                            _rawData[offset + (i * 2)] = tmpBytes[0];
                            _rawData[offset + (i * 2) + 1] = tmpBytes[1];
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _rwLock?.Dispose();
                }

                disposedValue = true;
            }
        }

        ~FileDataBlock()
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