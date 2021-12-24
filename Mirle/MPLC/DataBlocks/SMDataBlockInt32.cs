using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;

using Mirle.MPLC.DataBlocks.DeviceRange;

namespace Mirle.MPLC.DataBlocks
{
    public class SMDataBlockInt32 : SMDataBlock
    {
        private readonly int _startOffset = 4;

        public SMDataBlockInt32(ITypeDeviceRange deviceRange, string sharedMemoryName) : base(deviceRange, sharedMemoryName)
        {
        }

        protected override void Initial()
        {
            _mmfLength = DeviceRange.ByteArrayLength;
            _mmf = MemoryMappedFile.CreateOrOpen(_mmfName, _startOffset + (_mmfLength * 2));
        }

        public override void SetRawData(byte[] newRawData)
        {
            try
            {
                _rwLock.EnterWriteLock();
                var stream = _mmf.CreateViewStream();
                using (var writer = new BinaryWriter(stream))
                {
                    stream.Seek(_startOffset, SeekOrigin.Begin);
                    for (int i = 0; i < Math.Min(newRawData.Length, _mmfLength); i += 2)
                    {
                        writer.Write(newRawData[i]);
                        writer.Write(newRawData[i + 1]);
                        stream.Seek(2, SeekOrigin.Current);
                    }
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public override byte[] GetRawData()
        {
            byte[] data = new byte[_mmfLength];
            try
            {
                _rwLock.EnterReadLock();
                var stream = _mmf.CreateViewStream();
                using (var reader = new BinaryReader(stream))
                {
                    stream.Seek(_startOffset, SeekOrigin.Begin);
                    for (int i = 0; i < _mmfLength; i += 2)
                    {
                        data[i] = reader.ReadByte();
                        data[i + 1] = reader.ReadByte();
                        stream.Seek(2, SeekOrigin.Current);
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return data;
        }

        public override bool TryGetBit(string address, out bool value)
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
                        stream.Seek(_startOffset + (offset * 2), SeekOrigin.Begin);
                        ushort word = reader.ReadUInt16();

                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        value = bitArray.Get(index);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        protected override bool SetBit(string address, bool IsOn)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset) && DeviceRange.TryGetByteArrayBitIndex(address, out int index))
                {
                    int startOffset = _startOffset + (offset * 2);
                    var stream = _mmf.CreateViewStream();
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(startOffset, SeekOrigin.Begin);
                        ushort word = reader.ReadUInt16();

                        var bitArray = new BitArray(BitConverter.GetBytes(word));
                        bitArray.Set(index, IsOn);

                        byte[] tmpBytes = new byte[2];
                        bitArray.CopyTo(tmpBytes, 0);

                        stream = _mmf.CreateViewStream();
                        using (var writer = new BinaryWriter(stream))
                        {
                            stream.Seek(startOffset, SeekOrigin.Begin);
                            writer.Write(tmpBytes[0]);
                            writer.Write(tmpBytes[1]);
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            return false;
        }

        public override bool TryGetWord(string address, out int value)
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
                        stream.Seek(_startOffset + (offset * 2), SeekOrigin.Begin);
                        value = reader.ReadUInt16();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        public override bool TrySetWord(string address, int value)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var writer = new BinaryWriter(stream))
                    {
                        stream.Seek(_startOffset + (offset * 2), SeekOrigin.Begin);
                        byte[] tmpBytes = BitConverter.GetBytes(value);
                        writer.Write(tmpBytes[0]);
                        writer.Write(tmpBytes[1]);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return false;
        }

        public override bool TryGetWords(string address, out int[] data, int length)
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
                        stream.Seek(_startOffset + (offset * 2), SeekOrigin.Begin);
                        for (int i = 0; i < length; i++)
                        {
                            data[i] = reader.ReadUInt16();
                            stream.Seek(2, SeekOrigin.Current);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return false;
        }

        public override bool TrySetWords(string address, int[] data)
        {
            try
            {
                _rwLock.EnterWriteLock();
                if (DeviceRange.TryGetByteArrayOffset(address, out int offset))
                {
                    var stream = _mmf.CreateViewStream();
                    using (var writer = new BinaryWriter(stream))
                    {
                        stream.Seek(_startOffset + (offset * 2), SeekOrigin.Begin);
                        for (int i = 0; i < data.Length; i++)
                        {
                            byte[] tmpBytes = BitConverter.GetBytes(data[i]);
                            writer.Write(tmpBytes[0]);
                            writer.Write(tmpBytes[1]);
                            stream.Seek(2, SeekOrigin.Current);
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex.Message}-{ex.StackTrace}"); }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return false;
        }
    }
}