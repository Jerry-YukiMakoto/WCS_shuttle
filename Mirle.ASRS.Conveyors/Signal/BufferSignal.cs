
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class BufferSignal
    {
        public int BufferIndex { get; }
        public string BufferName { get; }
        public Word CommandId { get; protected internal set; }
        public Word PathChangeNotice { get; protected internal set; }
        public Word CmdMode { get; protected internal set; }
        public BufferStatusSignal StatusSignal { get; protected internal set; }
        public Word Alarm { get; protected internal set; }
        public Word Ready { get; protected internal set; }
        public Word Switch_Ack { get; protected internal set; }
        public Word A2LV2 { get; protected internal set; }
        public Word EmptyInReady { get; protected internal set; }
        public Word InitialNotice { get; protected internal set; }

        public BufferControllerSignal ControllerSignal { get; }

        public BufferSignal(int bufferIndex, string bufferName)
        {
            BufferIndex = bufferIndex;
            BufferName = bufferName;
            StatusSignal = new BufferStatusSignal();
            ControllerSignal = new BufferControllerSignal();
        }
    }
}
