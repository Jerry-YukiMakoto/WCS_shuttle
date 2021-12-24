namespace Mirle.ASRS.AWCS.Model.PLCDefinitions
{
    public struct LoadCategory
    {
        public const int EmptyTray = 1;
        public const int EmptyTrayWithPartition = 2;
        public const int EmptyTrayWithDampingMaterial = 3;
        public const int EmptyFOSBWithPartition = 4;
        public const int EmptyFOUPWithDampingMaterial = 5;
        public const int FOSB = 6;
        public const int FOUP = 7;
        public const int Material = 8;
        public const int Unknow = 9;
    }
}
