using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunicationPLC.Siemens;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using PLCConfigSetting.PLCsetting;

namespace Mirle.DB.Object.Service
{
    public class clsStoreOut
    {
        public static void StoreOut_WriteCV(clsBufferData Plc1)
        {
            try
            {
                //for (int i = 0; i <= 1; i++)
                //{
                //    if (i == 0)
                //    {

                //    }
                //    else
                //    {

                //    }
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutWriPlc1FA2andA4(Plc1);
                //}
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }
        public static void FunPickUpCmdtoSHC(clsBufferData Plc1)
        {
            try
            {

                clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutStart_GiveSHCCommand(Plc1);

            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void FunDoubletoSHC(clsBufferData Plc1)
        {
            try
            {

                clsDB_Proc.GetDB_Object().GetProcess().FunDoubleStorage_GiveSHCCommand(Plc1);

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
