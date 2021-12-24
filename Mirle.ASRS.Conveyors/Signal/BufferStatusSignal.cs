
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class BufferStatusSignal
    {
        public Bit InMode { get; protected internal set; }
        public Bit OutMode { get; protected internal set; }
        public Bit Error { get; protected internal set; }
        public Bit Auto { get; protected internal set; }
        public Bit Manual { get; protected internal set; }
        public Bit Presence { get; protected internal set; }
        public Bit Position { get; protected internal set; }
        public Bit Finish { get; protected internal set; }
        public Bit EmergencyStop { get; protected internal set; }
        public Bit AutomaticDoor { get; protected internal set; }

        public Bit LoadReq { get; protected internal set; }
        public Bit LoadFinish { get; protected internal set; }
        public Bit UnloadReq { get; protected internal set; }
        public Bit UnloadFinish { get; protected internal set; }
    }
}
