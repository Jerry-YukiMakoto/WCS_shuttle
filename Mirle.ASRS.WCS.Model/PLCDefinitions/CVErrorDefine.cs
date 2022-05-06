using System.Collections.Generic;

namespace Mirle.ASRS.WCS.Model.PLCDefinitions
{
    public struct CVErrorDefine
    {

        public static Dictionary<int, string> bitError { get; set; }

        public static Dictionary<int, string> A2bitError { get; set; }
        public static Dictionary<int, string> A4bitError { get; set; }
        public static Dictionary<int, string> PortStnbitError { get; set; }
    }
}
