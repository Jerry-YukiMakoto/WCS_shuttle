
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class SystemControllerSignal
    {
        public Word Heartbeat { get; protected internal set; }
        public WordBlock SystemTimeCalibration { get; protected internal set; }
    }
}
