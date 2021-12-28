namespace Mirle.ASRS.AWCS.Model.DataAccess
{
    public struct Trace
    {
        public const string StoreOutWriteCraneCmdToCV = "11";
        public const string StoreOutCreateCraneCmd = "12";
        public const string StoreOutCraneCmdFinish = "13";

        public const string StoreInWriteCmdToCV = "21";
        public const string StoreInCreateCraneCmd = "23";
        public const string StoreInCraneCmdFinish = "24";

        public const string EmptyStoreOutWriteCraneCmdToCV = "31";
        public const string EmptyStoreOutCreateCraneCmd = "32";
        public const string EmptyStoreOutCraneCmdFinish = "33";

        public const string EmptyStoreInWriteCraneCmdToCV = "41";
        public const string EmptyStoreInCreateCraneCmd = "42";
        public const string EmptyStoreInCraneCmdFinish = "43";

        public const string LoctoLocReady = "51";
        public const string LoctoLocReadyFinish = "52";
    }
}
