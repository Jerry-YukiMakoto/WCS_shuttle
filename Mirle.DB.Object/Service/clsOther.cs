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
        public class clsEmptyPallets
        {
            public static void EmptyStoreIn_A1_WriteCV()
            {
                try
                {
                    int bufferIndex = 4;
                    clsDB_Proc.GetDB_Object().GetProcess().FunEmptyStoreInWriPlc(StnNo.A4, bufferIndex);
                }
                catch (Exception ex)
                {
                    int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                    clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                }
            }

            public static void EmptyStoreIn_A1_CreateEquCmd()
            {
                try
                {
                    int bufferIndex = 1;
                    clsDB_Proc.GetDB_Object().GetProcess().FunEmptyStoreInCreateEquCmd(bufferIndex);
                }
                catch (Exception ex)
                {
                    int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                    clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                }
            }

            public static void EmptyStoreIn_EquCmdFinish()
            {
                try
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunEmptyStoreInEquCmdFinish();
                }
                catch (Exception ex)
                {
                    int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                    clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                }
            }

            public static void EmptyStoreOut_A1_WriteCV()
            {
                try
                {
                    int bufferIndex = 1;
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutWriPlc(StnNo.A4, bufferIndex);
                }
                catch (Exception ex)
                {
                    int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                    clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                }
            }

            //public static void EmptyStoreOut_A1_CreateEquCmd()
            //{
            //    try
            //    {
            //        int bufferIndex = 1;
            //        clsDB_Proc.GetDB_Object().GetProcess().FunStoreOutCreateEquCmd(bufferIndex);
            //    }
            //    catch (Exception ex)
            //    {
            //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            //    }
            //}

            //public static void EmptyStoreOut_EquCmdFinish()
            //{
            //    try
            //    {
            //        clsDB_Proc.GetDB_Object().GetProcess().FunEmptyStoreOutEquCmdFinish();
            //    }
            //    catch (Exception ex)
            //    {
            //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            //    }
            //}
        }
        
        public class clsL2L
        {
            public static void Other_LocToLoc()
            {
                try
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunLocToLoc();
                }
                catch (Exception ex)
                {
                    int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                    clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                }
            }

            public static void Other_LocToLocfinish()
            {
                try
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunLocToLocCmdFinish();
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
}
