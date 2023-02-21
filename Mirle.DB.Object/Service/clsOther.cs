using HslCommunicationPLC.Siemens;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DB.Object.Service
{
    public class clsOther
    {

        public static void Fun_L2L(PLCConfigSetting.PLCsetting.clsBufferData Plc1)//對lifter寫入命令
        {
            try
            {
                clsDB_Proc.GetDB_Object().GetProcess().FunL2L(Plc1);

            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }
    }
}
