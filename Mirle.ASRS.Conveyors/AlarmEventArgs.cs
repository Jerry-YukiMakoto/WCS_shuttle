using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.ASRS.Conveyors
{
    public class AlarmEventArgs : BufferEventArgs
    {
        public int AlarmBit { get; }

        public AlarmEventArgs(int bufferIndex, string bufferName, int alarmBit) : base(bufferIndex, bufferName)
        {
            AlarmBit = alarmBit;
        }
    }
}
