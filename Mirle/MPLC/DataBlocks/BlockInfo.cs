using Mirle.MPLC.DataBlocks.DeviceRange;

namespace Mirle.MPLC.DataBlocks
{
    public class BlockInfo
    {
        public string StartAddress { get; }
        public string EndAddress { get; }
        public string SharedMemoryName { get; }
        public int PLCRawdataIndex { get; }
        public ITypeDeviceRange DeviceRange { get; }

        public BlockInfo(ITypeDeviceRange deviceRange, string sharedMemoryName, int plcRawdataIndex)
        {
            DeviceRange = deviceRange;
            StartAddress = deviceRange.StartAddress;
            EndAddress = deviceRange.EndAddress;
            SharedMemoryName = sharedMemoryName;
            PLCRawdataIndex = plcRawdataIndex;
        }
    }
}