using System;
using Mirle.Structure;
using System.Windows.Forms;
using Mirle.Structure.Info;
using Mirle.Def;
using System.Collections.Generic;
//using Mirle.Def.U2NMMA30;
//using Mirle.Micron.U2NMMA30;
//using Mirle.Grid.U2NMMA30;

namespace Mirle.DB.Object
{
    /*
    public class clsDeposit
    {
        public static bool SubDepositToInPortCV_Proc(DataGridViewRow row, int fork)
        {
            try
            {
                ConveyorInfo buffer = new ConveyorInfo();
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                    if (fork == 1)
                    {
                        if (clsMicronCV.CheckInPortIsStockOutReady_ForLeftFork(int.Parse(sCurDeviceID), ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(row, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{sCurDeviceID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        if (clsMicronCV.CheckInPortIsStockOutReady_ForRightFork(int.Parse(sCurDeviceID), ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(row, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{sCurDeviceID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool SubDepositToOutPortCV_Proc(DataGridViewRow row, int fork)
        {
            try
            {
                ConveyorInfo buffer = new ConveyorInfo();
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                    if (fork == 1)
                    {
                        if (clsMicronCV.CheckOutPortIsStockOutReady_ForLeftFork(int.Parse(sCurDeviceID), ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(row, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{sCurDeviceID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        if (clsMicronCV.CheckOutPortIsStockOutReady_ForRightFork(int.Parse(sCurDeviceID), ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(row, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{sCurDeviceID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool SubDepositToInPortCV_Proc(TransferBatch cmd, int StockerID, int fork)
        {
            try
            {
                ConveyorInfo buffer = new ConveyorInfo();
                string sCmdSno = cmd.CommandID;
                string sRemark_Pre = cmd.Remark;
                string sRemark = "";

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    if (fork == 1)
                    {
                        if (clsMicronCV.CheckInPortIsStockOutReady_ForLeftFork(StockerID, ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(cmd, StockerID, fork, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{StockerID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        if (clsMicronCV.CheckInPortIsStockOutReady_ForRightFork(StockerID, ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(cmd, StockerID, fork, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{StockerID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool SubDepositToOutPortCV_Proc(TransferBatch cmd, int StockerID, int fork)
        {
            try
            {
                ConveyorInfo buffer = new ConveyorInfo();
                string sCmdSno = cmd.CommandID;
                string sRemark_Pre = cmd.Remark;
                string sRemark = "";

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    if (fork == 1)
                    {
                        if (clsMicronCV.CheckOutPortIsStockOutReady_ForLeftFork(StockerID, ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(cmd, StockerID, fork, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{StockerID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        if (clsMicronCV.CheckOutPortIsStockOutReady_ForRightFork(StockerID, ref buffer))
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToCV_Proc(cmd, StockerID, fork, buffer);
                        }
                        else
                        {
                            sRemark = $"Error: Stocker{StockerID}的CV皆無出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunDepositToCV_Proc(int StockerID, int Fork)
        {
            try
            {
                string sRemark = "";
                string sRemark_Pre = clsMicronStocker.GetCommand(StockerID, Fork).Remark;
                string sCmdSno = clsMicronStocker.GetCommand(StockerID, Fork).CommandID;

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    if (clsMicronCV.StockOutPortIsAllNoReady(StockerID))
                    {
                        sRemark = $"Error: Stocker{StockerID}的CV皆無出庫Ready！";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    else
                    {
                        #region 左側
                        if (SubDepositToOutPortCV_Proc(clsMicronStocker.GetCommand(StockerID, Fork), StockerID, Fork)) return true;
                        else return false;
                        #endregion 左側
                    }
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunBatchCmdDeposit_Proc(IEnumerable<DataGridViewRow> obj_Batch)
        {
            try
            {
                foreach (var row in obj_Batch)
                {
                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                    if(sCurLoc != LocationDef.Location.LeftFork.ToString() &&
                       sCurLoc != LocationDef.Location.RightFork.ToString())
                    {
                        string sRemark = "Error: 等候前一筆設備命令過帳完成";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                }

                foreach (var row in obj_Batch)
                {
                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    string sRemark = "";
                    string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                    string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                    string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                    string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                    int fork = sCurLoc == LocationDef.Location.LeftFork.ToString() ? 1 : 2;
                    string BatchID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BatchID.Index].Value);
                    string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);

                    if (sCmdMode == clsConstValue.CmdMode.StockOut)
                    {   //出庫
                        if (!clsMicronCV.GetConveyorController().IsConnected)
                        {
                            sRemark = "Error: CV PLC連線異常！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(sCurDeviceID))
                            {
                                sRemark = "Error: CurDeviceID為空值，請檢查！";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                return false;
                            }

                            if (FunDepositToCV_Proc(int.Parse(sCurDeviceID), fork)) continue;
                            else return false;
                        }
                    }
                    else
                    {   //只有出庫才能Batch
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(sCmdMode, BatchID);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                int errorLine = ex.StackTrace.IndexOf("行號");
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }
    }
    */
}
