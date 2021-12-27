
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class BufferControllerSignal 
    {
        public Word CommandId { get; protected internal set; }

        public Word PathNotice { get; protected internal set; }
        public Word LoadCategory { get; protected internal set; }
        public Word TrayType { get; protected internal set; }
        public Word PathChangeNotice { get; protected internal set; }
        public Word InitialNotice { get; protected internal set; }
        public WordBlock CommandData { get; protected internal set; }

        public Word A4Emptysupply { get; protected internal set; }

        public Word Switch_Mode { get; protected internal set; }

        public Bit AutomaticDoorOpend { get; protected internal set; }
        public Bit AutomaticDoorClosed { get; protected internal set; }
    }
}
