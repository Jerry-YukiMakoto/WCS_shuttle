using HslCommunicationPLC.Siemens;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.IASC;
using Mirle.BarcodeReader;

namespace Mirle.DB.Object.Service
{
  
    public class clsStoreIn
    {
        public ShuttleController? _shuttleController;
        public static void StoreIn_StartWriteCV(clsBufferData Plc1)
        {
            try
            {
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreiNwriPLCA1andA3(Plc1);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_CALL_LifterAndSHC(clsBufferData Plc1)//入庫對shuttle與lifter之間的的交握控制
        {
            try
            {
                string stn = "";
                for (int bufferIndex = 2; bufferIndex <= 4; bufferIndex += 2)
                {
                    if (bufferIndex == 2)
                    {
                        stn = ASRS_Setting.A2;
                    }
                    else if (bufferIndex == 4)
                    {
                        stn = ASRS_Setting.A4;
                    }
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreInFA1andA3CallLifterAndSHC(Plc1, stn, bufferIndex);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void StoreIn_CALL_LifterAndSHCCheckSHCreport(clsBufferData Plc1,CommandReceiveEventArgs e)//SHCcall命令狀態
        {
            try
            {
                string stn = "";
                for (int bufferIndex = 2; bufferIndex <= 4; bufferIndex += 2)
                {
                    if (bufferIndex == 2)
                    {
                        stn = ASRS_Setting.A2;
                    }
                    else if (bufferIndex == 4)
                    {
                        stn = ASRS_Setting.A4;
                    }
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreInFA1andA3CallLifterAndSHCheckcommand(Plc1, stn, bufferIndex, e);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }



        public static void StoreIn_CarInLifter_WriteCmdInLifter(clsBufferData Plc1)//對lifter寫入命令
        {
            try
            {

                for (int floor = 1; floor <= 11; floor++)
                {
                    clsDB_Proc.GetDB_Object().GetProcess().FunStoreCarInLifter_ReportSHC(Plc1, floor);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void FunSHC_ChangeLayerReq(clsBufferData Plc1, ChangeLayerEventArgsLayer e )//對lifter寫入命令
        {
            try
            {
                if(clsDB_Proc.GetDB_Object().GetProcess().FunSHC_ChangeLayerReq(Plc1,e)==false)
                {
                   var shuttle = new clsStoreIn();
                   shuttle._shuttleController?.S84("1", "0001");//Result_Code:0000=Succeess
                   shuttle._shuttleController?.P85("1", "F", "0001");//Result_Code:0000=Succeess
                }

            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void FunSHC_ChangeLayerReqtest(clsBufferData Plc1)//對lifter寫入命令
        {
            try
            {
                //if (clsDB_Proc.GetDB_Object().GetProcess().FunSHC_ChangeLayerReqtesting(Plc1) == false)
                //{
                //    //var shuttle = new clsStoreIn();
                //    //shuttle._shuttleController?.S84("1", "0001");//Result_Code:0000=Succeess
                //    //shuttle._shuttleController?.P85("1", "F", "0001");//Result_Code:0000=Succeess
                //}

            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void FunBCR_StoreINSocket(clsBufferData Plc1, SocketDataReceiveEventArgs e)//BCR入庫觸發
        {
            try
            {
                clsDB_Proc.GetDB_Object().GetProcess().FunStoreInWriPlc1FBCR(Plc1, e);

            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        public static void shuttle(ShuttleController shuttleController)//BCR入庫觸發
        {
            try
            {
                clsDB_Proc.GetDB_Object().GetProcess().getshuttlecontroller(shuttleController);

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
