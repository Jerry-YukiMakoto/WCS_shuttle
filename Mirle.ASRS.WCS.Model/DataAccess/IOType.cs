namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public struct IOtype
    {
        public const int NormalstorIn = 1;
        public const int NormalstoreOut = 2;
        public const int R2R = 5;
        public const int EmptyStoreIn = 6; //只有A4用到
        public const int EmptyStroeOut = 7;//只有A4用到
        public const int Cycle = 9;//盤點出庫
        public const int EmptyStoreOutbyWMS = 11;//由WMS發起，單純出空棧板
    }
}