using System.Collections.Generic;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC.MCProtocol
{
    public class PLCHostInfo
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public IEnumerable<BlockInfo> BlockInfos { get; set; }
        public string HostId { get; set; }
    }
}