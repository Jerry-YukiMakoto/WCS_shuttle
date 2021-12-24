using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class SystemSignal
    {
        public Word Heartbeat { get; protected internal set; }
        public Word Alarm { get; protected internal set; }
        public SystemControllerSignal ControllerSignal { get;  }

        public SystemSignal()
        {
            ControllerSignal = new SystemControllerSignal();
        }
    }
}
