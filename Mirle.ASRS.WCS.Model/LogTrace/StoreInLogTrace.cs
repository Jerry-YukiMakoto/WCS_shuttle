namespace Mirle.ASRS.WCS.Model.LogTrace
{
    public class StoreInLogTrace : LogTraceBase
    {
        public string CmdSno { get; set; }
        public string TrayId { get; set; }

        public int TrayWeight { get; set; }

        public string Plant { get; set; }
        public string Plant_Left { get; set; }
        public string Plant_Right { get; set; }

        public string FosbId { get; set; }
        public string FosbId_Left { get; set; }
        public string FosbId_Right { get; set; }

        public string Source { get; set; }
        public string Dest { get; set; }

        public StoreInLogTrace(int bufferIndex, string bufferName, string otherMessage) : base(bufferIndex, bufferName, otherMessage)
        {
        }

        protected override string DefaultLogTrace()
        {
            string messge = $"{CmdSno}|{TrayId}|{TrayWeight}|";
            if (string.IsNullOrWhiteSpace(FosbId) == false)
            {
                messge += $"{FosbId}|";
            }
            else if (string.IsNullOrWhiteSpace(FosbId_Left) == false && string.IsNullOrWhiteSpace(FosbId_Right) == false)
            {
                messge += $"{FosbId_Left}|{Plant_Right}|";
            }
            else if (string.IsNullOrWhiteSpace(FosbId_Left) == false && string.IsNullOrWhiteSpace(FosbId_Right))
            {
                messge += $"Plant Left:{FosbId_Left}|";
            }
            else if (string.IsNullOrWhiteSpace(FosbId_Left) && string.IsNullOrWhiteSpace(FosbId_Right) == false)
            {
                messge += $"Plant Right:{FosbId_Right}|";
            }
            if (string.IsNullOrWhiteSpace(Plant) == false)
            {
                messge += $"{Plant}|";
            }
            else if (string.IsNullOrWhiteSpace(Plant_Left) == false && string.IsNullOrWhiteSpace(Plant_Right) == false)
            {
                messge += $"{Plant_Left}|{Plant_Right}|";
            }
            else if (string.IsNullOrWhiteSpace(Plant_Left) == false && string.IsNullOrWhiteSpace(Plant_Right))
            {
                messge += $"Plant Left:{Plant_Left}|";
            }
            else if (string.IsNullOrWhiteSpace(Plant_Left) && string.IsNullOrWhiteSpace(Plant_Right) == false)
            {
                messge += $"Plant Right:{Plant_Right}|";
            }
            if (string.IsNullOrWhiteSpace(Source) == false)
            {
                messge += $"{Source}|{Dest}|";
            }
            messge += $"{OtherMessage}";
            return messge;
        }
    }
}
