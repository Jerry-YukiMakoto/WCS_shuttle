using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunicationPLC.Siemens;
using Mirle.ASRS.WCS.Model.PLCDefinitions;

namespace Mirle.DB.Object.Service
{
    public class clsStoreOut
    {
        public static void StoreOut_WriteCV(clsBufferData Plc1)
        {
            try
            {
                int bufferIndex = 2;

                for (bufferIndex = 2; bufferIndex <= 4; bufferIndex += 2)
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutWriPlc1FA2andA4(Plc1, bufferIndex);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }
        public static void FunPickUpCmdWritePLC(clsBufferData Plc1)
        {
            try
            {
                int bufferIndex = 2;

                for (bufferIndex = 2; bufferIndex <= 4; bufferIndex += 2)
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunPickUpCmdWritePLC(Plc1, bufferIndex);
                }
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
