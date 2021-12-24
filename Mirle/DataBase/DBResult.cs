namespace Mirle.DataBase
{
    public struct DBResult
    {
        public const int Success = 0;
        public const int Initial = 100001;
        public const int Exception = 100002;
        public const int Failed = 100003;

        public const int NoMatchDBType = 110001;
        public const int NoDataSelect = 110002;
        public const int NoDataUpdate = 110003;
    }
}
