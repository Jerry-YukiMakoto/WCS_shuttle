namespace Mirle.ASRS.AWCS.Model.DataAccess
{
    public struct Trace
    {
        public const string StoreOutWriteCraneCmdToCV = "11";
        public const string StoreOutCreateCraneCmd = "12";
        public const string StoreOutCraneCmdFinish = "13";
        public const string StoreOutWriteCraneCmdToRGV = "14";
        public const string StoreOutCreateEGVCmd = "15";
        public const string StoreOutRGVCmdFinish = "16";

        public const string StoreInWriteCmdToCV = "21";
        public const string StoreInTrayOnStation = "22";
        public const string StoreInCreateCraneCmd = "23";
        public const string StoreInCraneCmdFinish = "24";
        public const string StoreInCreateEGVCmd = "25";
        public const string StoreInRGVCmdFinish = "26";
    }
}
