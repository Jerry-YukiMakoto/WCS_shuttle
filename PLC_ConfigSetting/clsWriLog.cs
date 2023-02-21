using Mirle.WriLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCConfigSetting
{
    public class clsWriLog
    {
        public static clsLog PLCLog = new clsLog("PLCrawData", true);
        public static clsLog Log = new clsLog("HslCommunicationPLC", true);

        public static void PLCHistory(string rawdata)
        {
            PLCLog.FunWriTraceLog_CV(rawdata);
        }
    }
}
