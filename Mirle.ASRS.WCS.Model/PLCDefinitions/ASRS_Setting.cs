namespace Mirle.ASRS.WCS.Model.PLCDefinitions
{
    public struct ASRS_Setting
    {
        /// <summary>
        /// 左邊人工站Shuttle站號
        /// </summary>
        public static string STNNO_1F_left { get; set; }
        /// <summary>
        /// 左邊人工站WMS站號
        /// </summary>
        public static string STNNO_1F_L { get; set; }
        /// <summary>
        /// 右邊人工站Shuttle站號
        /// </summary>
        public static string STNNO_1F_right { get; set; }
        /// <summary>
        /// 右邊人工站WMS站號
        /// </summary>
        public static string STNNO_1F_R { get; set; }
        public static string BCR_IP_1 { get; set; }

        public static string BCR_IP_2 { get; set; }
        public static string BCR_port { get; set; }

        public const string A1 = "A1";
        public const string A2 = "A2";
        public const string A3 = "A3";
        public const string A4 = "A4";

    }
}
