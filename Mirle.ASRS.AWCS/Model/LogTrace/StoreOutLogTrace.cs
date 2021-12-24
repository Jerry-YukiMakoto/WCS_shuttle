namespace Mirle.ASRS.AWCS.Model.LogTrace
{
    public class StoreOutLogTrace : LogTraceBase
    {
        public string CmdSno { get; internal set; }
        public string TrayId { get; internal set; }
        public int LoadCategory { get; internal set; }

        public string Source { get; internal set; }
        public string Dest { get; internal set; }

        public StoreOutLogTrace(int bufferIndex, string bufferName, string otherMessage) : base(bufferIndex, bufferName, otherMessage)
        {
        }

        protected override string DefaultLogTrace()
        {
            string messge = $"{CmdSno}|{TrayId}|{LoadCategory}|";
            if (string.IsNullOrWhiteSpace(Source) == false)
            {
                messge += $"{Source}|{Dest}|";
            }
            messge += $"{OtherMessage}";
            return messge;
        }
    }
}
