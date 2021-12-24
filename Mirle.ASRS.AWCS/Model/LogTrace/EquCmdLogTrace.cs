namespace Mirle.ASRS.AWCS.Model.LogTrace
{
    public class EquCmdLogTrace : LogTraceBase
    {
        public int CraneNo { get; internal set; }
        public string CommandId { get; internal set; }
        public string Source { get; internal set; }
        public string Destination { get; internal set; }

        public EquCmdLogTrace(int bufferIndex, string bufferName, string otherMessage) : base(bufferIndex, bufferName, otherMessage)
        {
        }

        protected override string DefaultLogTrace()
        {
            return $"{CraneNo}|{CommandId}|{Source}|{Destination}";
        }
    }
}
