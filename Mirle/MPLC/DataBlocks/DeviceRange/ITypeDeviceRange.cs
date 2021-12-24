namespace Mirle.MPLC.DataBlocks.DeviceRange
{
    public interface ITypeDeviceRange
    {
        string StartAddress { get; }
        string EndAddress { get; }
        int WordLength { get; }
        int ByteArrayLength { get; }

        bool IsSameRange(string address);

        bool TryGetIndex(string address, out int index);

        bool TryGetOffset(string address, out int offset);

        bool TryGetByteArrayOffset(string address, out int offset);

        bool TryGetByteArrayBitIndex(string address, out int index);
    }
}