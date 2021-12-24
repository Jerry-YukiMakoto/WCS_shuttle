using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

using Mirle.MPLC.DataBlocks.DeviceRange;

namespace Mirle.MPLC.DataBlocks
{
    public class SMDataBlock : IDataBlock, IDisposable
    {
        protected readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        protected string _mmfName;
        protected long _mmfLength;
        protected MemoryMappedFile _mmf;

        public ITypeDeviceRange DeviceRange { get; }
        public string MemoryMappedName => _mmfName;

        public SMDataBlock(ITypeDeviceRange deviceRange, string sharedMemoryName)
        {
            DeviceRange = deviceRange;
            _mmfName = sharedMemoryName;

            Initial();
        }

        protected virtual void Initial()
        {
            _mmfLength = DeviceRange.ByteArrayLength;
            _mmf = MemoryMappedFile.CreateOrOpen(_mmfName, _mmfLength);
        }

        public virtual void SetRawData(byte[] newRawData)
        {
            try
            {
                _rwLock.EnterWriteLock();
                var stream = _mmf.CreateViewStream();
                using (var writer = new BinaryWriter(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    for (int i = 0; i < Math.Min(newRawData.Length, _mmfLength); i++)
                    {
                        writer.Write(newRawData[i]);
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
        }

        public virtual byte[] GetRawData()
        {
            byte[] data = new byte[_mmfLength];
            try
            {
                _rwLock.EnterReadLock();
                var stream = _mmf.CreateViewStream();
                using (var reader = new BinaryReader(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    for (int i = 0; i < _mmfLength; i++)
                    {
                        data[i] = reader.ReadByte();
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
            return data;
        }

        public virtual bool TryGetBit(string address, out bool value)
        {
            value = false;
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset) && DeviceRange.TryGetByteArrayBitIndex(address, out int index))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        ushort word = reader.ReadUInt16();

                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        value = bitArray.Get(index);
                    }
                    return true;
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

        protected virtual bool SetBit(string address, bool IsOn)
        {
            try
            {
                _rwLock.EnterWriteLock();

                if (DeviceRange.TryGetByteArrayOffset(address, out int offset) && DeviceRange.TryGetByteArrayBitIndex(address, out int index))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        ushort word = reader.ReadUInt16();

                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        bitArray.Set(index, IsOn);

                        byte[] tmpBytes = new byte[2];
                        bitArray.CopyTo(tmpBytes, 0);

                        stream = _mmf.CreateViewStream();
                        using (var writer = new BinaryWriter(stream))
                        {
                            stream.Seek(offset, SeekOrigin.Begin);
                            writer.Write(tmpBytes[0]);
                            writer.Write(tmpBytes[1]);
                        }
                    }

                    return true;
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

        public virtual bool TryGetWord(string address, out int value)
        {
            value = 0;
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        value = reader.ReadUInt16();
                    }
                    return true;
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

        public virtual bool TrySetWord(string address, int value)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var writer = new BinaryWriter(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        byte[] tmpBytes = BitConverter.GetBytes(value);
                        writer.Write(tmpBytes[0]);
                        writer.Write(tmpBytes[1]);
                    }
                    return true;
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

        public virtual bool TryGetWords(string address, out int[] data, int length)
        {
            data = new int[length];
            try
            {
                _rwLock.EnterReadLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        for (int i = 0; i < length; i++)
                        {
                            data[i] = reader.ReadUInt16();
                        }
                    }
                    return true;
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

        public virtual bool TrySetWords(string address, int[] data)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var writer = new BinaryWriter(stream))
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        for (int i = 0; i < data.Length; i++)
                        {
                            byte[] tmpBytes = BitConverter.GetBytes(data[i]);
                            writer.Write(tmpBytes[0]);
                            writer.Write(tmpBytes[1]);
                        }
                    }
                    return true;
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
                    _mmf?.Dispose();
                    _rwLock?.Dispose();
                }
                disposedValue = true;
            }
        }

        ~SMDataBlock()
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