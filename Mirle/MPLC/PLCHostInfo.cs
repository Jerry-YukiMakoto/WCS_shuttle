using System.Collections.Generic;

using Mirle.MPLC.DataBlocks;

namespace Mirle.MPLC
{
    public class PLCHostInfo
    {
        public string HostId { get; set; }

        public string IPAddress { get; set; }
        public int TcpPort { get; set; }

        public int ActLogicalStationNo { get; set; }

        public IEnumerable<BlockInfo> BlockInfos { get; set; }
    }
}
