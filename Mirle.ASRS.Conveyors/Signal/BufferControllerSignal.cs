
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class BufferControllerSignal 
    {
        public Word CommandId { get; protected internal set; }
        public Word CmdMode { get; protected internal set; }
        public Word PathChangeNotice { get; protected internal set; }
        public Word InitialNotice { get; protected internal set; }
        public Word A4Emptysupply { get; protected internal set; }
        public Word Switch_Mode { get; protected internal set; }

    }
}
