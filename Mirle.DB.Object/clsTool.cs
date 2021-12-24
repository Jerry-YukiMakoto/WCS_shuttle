using System;
using System.Linq;
using Mirle.Def;
using Mirle.Structure;
using Mirle.Micron.U2NMMA30;
using Mirle.ASRS.Conveyor.U2NMMA30;
using System.Data;
using Mirle.DataBase;

namespace Mirle.DB.Object
{
    public class clsTool
    {
        public static bool CheckSts(int StockerID, ref string sRemark)
        {
            if (!clsMicronStocker.GetSTKCHostById(StockerID).GetSTKCManager().IsPlcConn)
            {
                sRemark = $"Error: Stocker{StockerID}的PLC未連線！";
                return false;
            }

            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).IsInService)
            {
                sRemark = $"Error: Stocker{StockerID} OutService！";
                return false;
            }

            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).IsIdle)
            {
                sRemark = $"Error: Stocker{StockerID}並非IDLE！";
                return false;
            }

            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).ReadyRecieveNewCommand)
            {
                sRemark = $"Error: Stocker{StockerID}不允許下新命令！";
                return false;
            }

            return true;
        }

        public static bool CheckSts(int StockerID, string sCmdSno, string sRemark_Pre, ref string sRemark)
        {
            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).IsInService)
            {
                sRemark = $"Error: Stocker{StockerID} OutService！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }

                return false;
            }

            if (!clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetForkCommand(StockerID))
            {
                sRemark = $"Error: Stocker{StockerID} => 取得Fork命令失敗！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }

                return false;
            }

            return true;
        }

        public static bool CheckSts_ForStockInBegin(int StockerID, string sCmdSno, string sRemark_Pre, ref string sRemark)
        {
            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).IsInService)
            {
                sRemark = $"Error: Stocker{StockerID} OutService！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }

                return false;
            }

            return true;
        }

        public static bool CheckCraneIsIdle(int StockerID, string sCmdSno, string sRemark_Pre, ref string sRemark)
        {
            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).IsIdle)
            {
                sRemark = $"Error: Stocker{StockerID}並非IDLE！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }

                return false;
            }

            if (!clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).ReadyRecieveNewCommand)
            {
                sRemark = $"Error: Stocker{StockerID}不允許下新命令！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }

                return false;
            }

            return true;
        }

        public static bool GetNewStnForStockOut(CmdMstInfo cmd, ConveyorInfo buffer, ref string StnNo_New, 
            ref bool IsChangeBackupPort, ref int Path)
        {
            try
            {
                return clsMicronCV.GetPathByStockOut(cmd.StnNo, cmd.backupPortId, buffer.StnNo, ref IsChangeBackupPort, ref StnNo_New, ref Path);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool RefreshCanGoForStockOut()
        {
            int CycleCount = 0;
            for (int i = 0; i < ConveyorController.CycleIndex.Length; i++)
            {
                var buffer = clsMicronCV.GetConveyorController().GetBuffer(ConveyorController.CycleIndex[i]);
                if (buffer.Presence || !string.IsNullOrWhiteSpace(buffer.CommandID)) CycleCount++;
            }

            DataTable dtTmp_ForkCmd = new DataTable();
            int iRet = clsDB_Proc.GetDB_Object().GetTask().GetForkPickupCmd_ForStockOut(ref dtTmp_ForkCmd);
            bool bFlag;
            if (iRet == DBResult.Success || iRet == DBResult.NoDataSelect)
            {
                bFlag = true;
                if (dtTmp_ForkCmd != null)
                {
                    CycleCount += dtTmp_ForkCmd.Rows.Count;
                }
            }
            else bFlag = false;

            ConveyorController.CycleCount = CycleCount;
            return bFlag;
        }

        public static bool CheckCanGoForStockOut(string sCmdSno, string sRemark_Pre, ref string sRemark)
        {
            if (!RefreshCanGoForStockOut())
            {
                sRemark = $"Error: 檢查水位時，取得Fork命令失敗";
                if (sRemark_Pre != sRemark)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }
            }

            clsWriLog.Log.FunWriTraceLog_CV($"<目前大循環數量>{ ConveyorController.CycleCount} <大循環水位最大值>{ConveyorController.CycleCountMax}");
            if (ConveyorController.CycleCount >= ConveyorController.CycleCountMax) return false;
            else return true;
        }


        public static bool CheckCanGoForTicket(string sCmdSno, string sLoc, string sTicketId, string sStnNo, int iEquNo, string sRemark_Pre, ref string sRemark)
        {
            string smlTicketId = "";

            int iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetSmallTicketId(sStnNo, sTicketId, ref smlTicketId);
            if (iRet == Mirle.DataBase.DBResult.Success)
            {
                if (sTicketId == "" || sTicketId == " " || sTicketId == smlTicketId)
                {
                    return true;
                }
                else
                {
                    if (iEquNo != 4)
                    {
                        bool bCheckOutside = false;
                        string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                        iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sLoc, ref bCheckOutside,
                                        ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                        if (iRet == Mirle.DataBase.DBResult.Success)
                        {
                            if (!bCheckOutside)
                            {
                                if (!IsEmpty_DD)
                                {
                                    CmdMstInfo cmd_DD = new CmdMstInfo();
                                    iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                                    if (iRet == Mirle.DataBase.DBResult.Success)
                                    {
                                        if (cmd_DD.ticketId == smlTicketId)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            sRemark = $"Error: 等待執行中或較小訂單號{smlTicketId}完成";
                                            if (sRemark != sRemark_Pre)
                                            {
                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                        }
                                        return false;
                                    }
                                }
                                else
                                {
                                    sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }
                                return false;
                            }
                        }
                        else
                        {
                            sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }
                            return false;
                        }
                    }
                    else
                    {
                        sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }
                        return false;
                    }
                }
            }
            else if (iRet == Mirle.DataBase.DBResult.Exception)
            {
                sRemark = $"Error: 取得Port {sStnNo} 執行中的訂單號失敗！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }
                return false;
            }
            else
            {
                sRemark = $"Port {sStnNo} 沒有需要執行的訂單號！可直接搬運！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }
                return true;
            }
        }

        public static bool CheckCanGoForTicket(string sCmdSno, string sLoc, string sTicketId, string sStnNo, int iEquNo, string sCmdMode, string BatchID, string sRemark_Pre, ref string sRemark)
        {
            string smlTicketId = "";

            int iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetSmallTicketId(sStnNo, sTicketId, ref smlTicketId);
            if (iRet == Mirle.DataBase.DBResult.Success)
            {
                if (sTicketId == "" || sTicketId == " " || sTicketId == smlTicketId)
                {
                    return true;
                }
                else
                {
                    if (iEquNo != 4)
                    {
                        bool bCheckOutside = false;
                        string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                        iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sLoc, ref bCheckOutside,
                                        ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                        if (iRet == Mirle.DataBase.DBResult.Success)
                        {
                            if (!bCheckOutside)
                            {
                                if (!IsEmpty_DD)
                                {
                                    CmdMstInfo cmd_DD = new CmdMstInfo();
                                    iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                                    if (iRet == Mirle.DataBase.DBResult.Success)
                                    {
                                        if (cmd_DD.ticketId == smlTicketId)
                                        {
                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(sCmdMode, BatchID);
                                            return false;
                                        }
                                        else
                                        {
                                            sRemark = $"Error: 等待執行中或較小訂單號{smlTicketId}完成";
                                            if (sRemark != sRemark_Pre)
                                            {
                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                        }
                                        return false;
                                    }
                                }
                                else
                                {
                                    sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }
                                return false;
                            }
                        }
                        else
                        {
                            sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }
                            return false;
                        }
                    }
                    else
                    {
                        sRemark = $"Error: 等待執行中訂單號{smlTicketId}完成";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }
                        return false;
                    }
                }
            }
            else if (iRet == Mirle.DataBase.DBResult.Exception)
            {
                sRemark = $"Error: 取得Port {sStnNo} 執行中的訂單號失敗！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }
                return false;
            }
            else
            {
                sRemark = $"Port {sStnNo} 沒有需要執行的訂單號！可直接搬運！";
                if (sRemark != sRemark_Pre)
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                }
                return true;
            }
        }
    }
}
