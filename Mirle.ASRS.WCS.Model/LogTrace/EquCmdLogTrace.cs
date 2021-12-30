namespace Mirle.ASRS.WCS.Model.LogTrace
{
    public class EquCmdLogTrace : LogTraceBase
    {
        public int CraneNo { get; set; }
        public string CommandId { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        public EquCmdLogTrace(int bufferIndex, string bufferName, string otherMessage) : base(bufferIndex, bufferName, otherMessage)
        {
        }

        protected override string DefaultLogTrace()
        {
            return $"{CraneNo}|{CommandId}|{Source}|{Destination}";
        }
    }
}
