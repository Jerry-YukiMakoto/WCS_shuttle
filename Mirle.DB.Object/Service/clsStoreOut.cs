using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.ASRS.WCS.Model.PLCDefinitions;

namespace Mirle.DB.Object.Service
{
    public class clsStoreOut
    {
        public static void StoreOut_A1_WriteCV()
        {
            try
            {
                int bufferIndex = 1; //A1
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutWriPlc(StnNo.A3, bufferIndex);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreOut_A2ToA4_WriteCV()
        {
            try
            {
                string stn = "";
                for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
                {
                    if (bufferIndex == 5)
                    {
                        stn = StnNo.A5;
                    }
                    else if (bufferIndex == 7)
                    {
                        stn = StnNo.A7;
                    }
                    else if (bufferIndex == 9)
                    {
                        stn = StnNo.A9;
                    }
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutA2ToA4WriPlc(stn, bufferIndex);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreOut_A1_CreateEquCmd()
        {
            try
            {
                int bufferIndex = 1; //A1
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutCreateEquCmd(bufferIndex);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreOut_A2ToA4_CreateEquCmd()
        {
            try
            {
                for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutA2toA4CreateEquCmd(bufferIndex);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreOut_EquCmdFinish()
        {
            try
            {
                var stn = new List<string>()
                {
                    StnNo.A3,
                    StnNo.A6,
                    StnNo.A8,
                    StnNo.A10,
                };
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutEquCmdFinish(stn);
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
