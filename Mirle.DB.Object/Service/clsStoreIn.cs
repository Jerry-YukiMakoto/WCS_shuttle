using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DB.Object.Service
{
    public class clsStoreIn
    {
        public static void StoreIn_A1_WriteCV()
        {
            try
            {
                int bufferIndex = 3;
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreInWriPlc(StnNo.A3, bufferIndex);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_A2ToA4_WriteCV()//A2toA4寫入buffer
        {
            try
            {
                string stn = "";
                for (int bufferIndex = 6; bufferIndex <= 10; bufferIndex += 2)
                {
                    if (bufferIndex == 6)
                    {
                        stn = StnNo.A6;
                    }
                    else if (bufferIndex == 8)
                    {
                        stn = StnNo.A8;
                    }
                    else if (bufferIndex == 10)
                    {
                        stn = StnNo.A10;
                    }
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreInA2ToA4WriPlc(stn, bufferIndex);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_A1_CreateEquCmd()
        {
            try
            {
                int bufferIndex = 1;
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreInCreateEquCmd(bufferIndex);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_A2toA4_CreateEquCmd()
        {
            try
            {
                for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreInA2toA4CreateEquCmd(bufferIndex);
                }     
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_EquCmdFinish()
        {
            try
            {
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreInEquCmdFinish();  
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
