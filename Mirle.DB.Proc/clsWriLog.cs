using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.WriLog;

namespace Mirle.DB.Proc
{
    public class clsWriLog
    {
        public static clsLog Log = new clsLog("DB_Proc", true);
        public static void StoreInLogTrace(int BufferIndex, string BufferName, string Msg)
        {
            Log.FunWriTraceLog_CV($"{BufferIndex} | {BufferName}: {Msg}");
        }
    }
}
