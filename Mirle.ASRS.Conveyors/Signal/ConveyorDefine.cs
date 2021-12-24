using System;
using System.Collections.Generic;

namespace Mirle.ASRS.Conveyors.Signal
{
    [Serializable]
    public class ConveyorDefine
    {
        public int SignalGroup { get; set; }

        public List<BufferDefine> Buffers { get; set; }
    }

    [Serializable]
    public class BufferDefine
    {
        public int BufferIndex { get; set; }
        public string BufferName { get; set; }
    }
}
