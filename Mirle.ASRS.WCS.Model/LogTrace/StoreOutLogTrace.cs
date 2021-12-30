namespace Mirle.ASRS.WCS.Model.LogTrace
{
    public class StoreOutLogTrace : LogTraceBase
    {
        public string CmdSno { get; set; }
        public string TrayId { get; set; }
        public int LoadCategory { get; set; }

        public string Source { get; set; }
        public string Dest { get; set; }

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
