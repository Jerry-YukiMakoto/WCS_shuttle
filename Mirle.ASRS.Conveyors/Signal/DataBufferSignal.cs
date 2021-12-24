using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class DataBufferSignal
    {
        public DWord Weight { get; protected internal set; }
        public WordBlock TrayId { get; protected internal set; }
        public WordBlock FosbId { get; protected internal set; }
        public WordBlock Plant { get; protected internal set; }

        public WordBlock Plant_Left { get; protected internal set; }
        public WordBlock Plant_Right { get; protected internal set; }

        public WordBlock FosbId_Left { get; protected internal set; }
        public WordBlock FosbId_Right{ get; protected internal set; }
    }
}
