/*
using System;
using System.Linq;
using Mirle.Structure;
using Mirle.Def;
using Mirle.Grid.U2NMMA30;
using System.Windows.Forms;
using Mirle.Micron.U2NMMA30;
using Mirle.Def.U2NMMA30;
using Mirle.DataBase;
using System.Collections.Generic;

namespace Mirle.DB.Object
{
    public class clsPickUp
    {
        public static bool FunPickupForStockOut_Proc(DataGridViewRow row, int EquNo, int fork, ref string sRemark)
        {
            try
            {
                string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                string BatchID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);

                string sCmdSts = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                if (sCmdSts != clsConstValue.CmdSts.strCmd_Initial && sCurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).GetForkById(fork).HasCarrier)
                {
                    sRemark = $"Error: Stocker{EquNo}的Fork{fork}有物，請檢查！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                if (!Micron.U2NMMA30.clsTool.CheckForkCanDo(sLoc, EquNo, fork))
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(sCmdMode, BatchID);
                    sRemark = "Error: 因儲位在極限位置，故取消Batch";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                if(!clsMicronCV.GetConveyorController().IsConnected)
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                ConveyorInfo buffer = new ConveyorInfo();
                bool checkBuffer = clsMicronCV.GetStockOutBuffer(EquNo, fork, ref buffer, ref sRemark);
                if (!checkBuffer)
                {
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                Location Start = null;
                Location End = buffer.bufferLocation;
                #region 取得來源Location
                Start = MicronLocation.GetLocation_ByShelf(EquNo);
                if (Start == null)
                {
                    sRemark = $"Error: EquNo有誤，請檢查 => {EquNo}";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
                #endregion 取得來源Location
                Location sLoc_Start = null;
                Location sLoc_End = null;
                bool bCheck = MicronLocation.GetPath(Start, End, ref sLoc_Start, ref sLoc_End);
                if (bCheck == false)
                {
                    sRemark = "Error: Route給出的路徑為Null，WCS給的Location => Start: <Device>" + Start.DeviceId + " <Location>" + Start.LocationId +
                       "，End: <Device>" + End.DeviceId + " <Location>" + End.LocationId;
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                if (!clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, fork))
                {
                    sRemark = "Error: Batch的另一筆命令下達失敗！";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunPickupForStockOut_Proc(CmdMstInfo row, int fork, ref string sRemark)
        {
            try
            {
                if (row.CmdSts != clsConstValue.CmdSts.strCmd_Initial && row.CurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                if (clsMicronStocker.GetStockerById(Convert.ToInt32(row.EquNo)).GetCraneById(1).GetForkById(fork).HasCarrier)
                {
                    sRemark = $"Error: Stocker{row.EquNo}的Fork{fork}有物，請檢查！";
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }

                if (!Micron.U2NMMA30.clsTool.CheckForkCanDo(row.Loc, Convert.ToInt32(row.EquNo), fork))
                {
                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(row.CmdMode, row.BatchID);
                    sRemark = "Error: 因儲位在極限位置，故取消Batch";
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }

                if (!clsMicronCV.GetConveyorController().IsConnected)
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }

                ConveyorInfo buffer = new ConveyorInfo();
                bool checkBuffer = clsMicronCV.GetStockOutBuffer(Convert.ToInt32(row.EquNo), fork, ref buffer, ref sRemark);
                if (!checkBuffer)
                {
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }

                Location Start = null;
                Location End = buffer.bufferLocation;
                #region 取得來源Location
                Start = MicronLocation.GetLocation_ByShelf(Convert.ToInt32(row.EquNo));
                if (Start == null)
                {
                    sRemark = $"Error: EquNo有誤，請檢查 => {row.EquNo}";
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }
                #endregion 取得來源Location
                Location sLoc_Start = null;
                Location sLoc_End = null;
                bool bCheck = MicronLocation.GetPath(Start, End, ref sLoc_Start, ref sLoc_End);
                if (bCheck == false)
                {
                    sRemark = "Error: Route給出的路徑為Null，WCS給的Location => Start: <Device>" + Start.DeviceId + " <Location>" + Start.LocationId +
                       "，End: <Device>" + End.DeviceId + " <Location>" + End.LocationId;
                    if (sRemark != row.Remark)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(row.CmdSno, sRemark);
                    }

                    return false;
                }

                if (!clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, fork))
                {
                    sRemark = "Error: Batch的另一筆命令下達失敗！";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunPickupForStockOut_Proc_CmdMstInfo(CmdMstInfo row, int EquNo, ref string sRemark)
        {
            try
            {
                string sLoc = row.Loc;
                string sCmdMode = row.CmdMode;
                string BatchID = row.BatchID;
                string sRemark_Pre = row.Remark;
                string sCmdSno = row.CmdSno;

                string sCmdSts = row.CmdSts;
                string sCurLoc = row.CurLoc;
                if (sCmdSts != clsConstValue.CmdSts.strCmd_Initial && sCurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                if (!clsMicronCV.GetConveyorController().IsConnected)
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                for (int fork = 1; fork <= 2; fork++)
                {
                    var crane = clsMicronStocker.GetStockerById(EquNo).GetCraneById(1);

                    if (crane.GetForkById(fork).HasCarrier)
                    {
                        sRemark = $"Error: Stocker{EquNo}的Fork{fork}有物，請檢查！";
                        continue;
                    }

                    if (!crane.GetForkById(fork).GetConfig().Enable)
                    {
                        sRemark = $"Error: Stocker{EquNo}的Fork{fork}被Disable了！";
                        continue;
                    }

                    ConveyorInfo buffer = new ConveyorInfo();
                    bool checkBuffer = clsMicronCV.GetStockOutBuffer(EquNo, fork, ref buffer, ref sRemark);
                    if (!checkBuffer)
                    {
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        continue;
                    }

                    Location Start = null;
                    Location End = buffer.bufferLocation;
                    #region 取得來源Location
                    Start = MicronLocation.GetLocation_ByShelf(EquNo);
                    if (Start == null)
                    {
                        sRemark = $"Error: EquNo有誤，請檢查 => {EquNo}";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    #endregion 取得來源Location
                    Location sLoc_Start = null;
                    Location sLoc_End = null;
                    bool bCheck = MicronLocation.GetPath(Start, End, ref sLoc_Start, ref sLoc_End);
                    if (bCheck == false)
                    {
                        sRemark = "Error: Route給出的路徑為Null，WCS給的Location => Start: <Device>" + Start.DeviceId + " <Location>" + Start.LocationId +
                           "，End: <Device>" + End.DeviceId + " <Location>" + End.LocationId;
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }

                    if (!clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, fork))
                    {
                        sRemark = $"Error: {sCmdSno}命令下達失敗！";
                        return false;
                    }
                    else return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunPickupForStockOut_Proc(DataGridViewRow row, int EquNo, ref string sRemark)
        {
            try
            {
                string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                string BatchID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);

                string sCmdSts = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                if (sCmdSts != clsConstValue.CmdSts.strCmd_Initial && sCurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                if (!clsMicronCV.GetConveyorController().IsConnected)
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                for (int fork = 1; fork <= 2; fork++)
                {
                    var crane = clsMicronStocker.GetStockerById(EquNo).GetCraneById(1);

                    if (crane.GetForkById(fork).HasCarrier)
                    {
                        sRemark = $"Error: Stocker{EquNo}的Fork{fork}有物，請檢查！";
                        continue;
                    }

                    if (!crane.GetForkById(fork).GetConfig().Enable)
                    {
                        sRemark = $"Error: Stocker{EquNo}的Fork{fork}被Disable了！";
                        continue;
                    }

                    ConveyorInfo buffer = new ConveyorInfo();
                    bool checkBuffer = clsMicronCV.GetStockOutBuffer(EquNo, fork, ref buffer, ref sRemark);
                    if (!checkBuffer)
                    {
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        continue;
                    }

                    Location Start = null;
                    Location End = buffer.bufferLocation;
                    #region 取得來源Location
                    Start = MicronLocation.GetLocation_ByShelf(EquNo);
                    if (Start == null)
                    {
                        sRemark = $"Error: EquNo有誤，請檢查 => {EquNo}";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    #endregion 取得來源Location
                    Location sLoc_Start = null;
                    Location sLoc_End = null;
                    bool bCheck = MicronLocation.GetPath(Start, End, ref sLoc_Start, ref sLoc_End);
                    if (bCheck == false)
                    {
                        sRemark = "Error: Route給出的路徑為Null，WCS給的Location => Start: <Device>" + Start.DeviceId + " <Location>" + Start.LocationId +
                           "，End: <Device>" + End.DeviceId + " <Location>" + End.LocationId;
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }

                    if (!clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, fork))
                    {
                        sRemark = $"Error: {sCmdSno}命令下達失敗！";
                        return false;
                    }
                    else return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunPickupProc_ForStockOutBatch(IEnumerable<DataGridViewRow> obj_Batch, int EquNo, ref string sRemark)
        {
            try
            {
                TaskDTO[] taskInfo = new TaskDTO[obj_Batch.Count()];
                int i = 0;
                foreach (var batch_out in obj_Batch)
                {
                    string sCmdSts = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                    string sCurLoc = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                    if (sCmdSts != clsConstValue.CmdSts.strCmd_Initial && sCurLoc != LocationDef.Location.Teach.ToString())
                        return false;

                    taskInfo[i] = new TaskDTO();
                    string sLoc = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                    taskInfo[i].Remark = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    taskInfo[i].CommandID = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    taskInfo[i].Priority = Convert.ToInt32(batch_out.Cells[ColumnDef.CMD_MST.PRT.Index].Value);
                    taskInfo[i].StockerID = EquNo.ToString();
                    taskInfo[i].CSTID = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                    taskInfo[i].Source = Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(sLoc);
                    taskInfo[i].TransferMode = clsEnum.TaskMode.Pickup;

                    clsEnum.Fork fork = clsEnum.Fork.None;
                    if (Micron.U2NMMA30.clsTool.IsLimit(sLoc, ref fork)) taskInfo[i].ForkNo = (int)fork;
                    else taskInfo[i].ForkNo = 0;

                    i++;
                }

                if(taskInfo[0].ForkNo == taskInfo[1].ForkNo)
                {
                    if(taskInfo[0].ForkNo == 0)
                    {
                        taskInfo[0].ForkNo = (int)clsEnum.Fork.Left;
                        taskInfo[0].Destination = clsMicronStocker.GetShelfIdByFork(taskInfo[0].ForkNo);

                        taskInfo[1].ForkNo = (int)clsEnum.Fork.Right;
                        taskInfo[1].Destination = clsMicronStocker.GetShelfIdByFork(taskInfo[1].ForkNo);

                        return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_BatchProc(taskInfo, ref sRemark);
                    }
                    else
                    {
                        #region 極限儲位指定Fork
                        foreach (var batch_out in obj_Batch)
                        {
                            string sRemark_Pre = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                            string sCmdSno = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                            if (FunPickupForStockOut_Proc(batch_out, EquNo, taskInfo[0].ForkNo, ref sRemark)) return true;
                            else
                            {
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                return false;
                            }
                        }

                        return false;
                        #endregion 極限儲位指定Fork
                    }
                }
                else
                {
                    if(taskInfo[0].ForkNo == 0)
                    {
                        taskInfo[0].ForkNo = taskInfo[1].ForkNo == (int)clsEnum.Fork.Left ? (int)clsEnum.Fork.Right : (int)clsEnum.Fork.Left;
                    }
                    else if (taskInfo[1].ForkNo == 0)
                    {
                        taskInfo[1].ForkNo = taskInfo[0].ForkNo == (int)clsEnum.Fork.Left ? (int)clsEnum.Fork.Right : (int)clsEnum.Fork.Left;
                    }
                    else { }

                    for (int f = 0; f < taskInfo.Length; f++)
                    {
                        taskInfo[f].Destination = clsMicronStocker.GetShelfIdByFork(taskInfo[f].ForkNo);
                    }

                    return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_BatchProc(taskInfo, ref sRemark);
                }
            }
            catch (Exception ex)
            {
                sRemark = ex.Message;
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// For出庫的取物流程
        /// </summary>
        /// <param name="batch_out"></param>
        /// <param name="EquNo"></param>
        /// <param name="sRemark"></param>
        /// <returns></returns>
        public static bool FunPickupProc_ForStockOut(DataGridViewRow batch_out, int EquNo, ref string sRemark)
        {
            try
            {
                string sLoc = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                string sRemark_Pre = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sCmdSno = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);

                string sCmdSts = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                string sCurLoc = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                if (sCmdSts != clsConstValue.CmdSts.strCmd_Initial && sCurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                clsEnum.Fork fork = clsEnum.Fork.None;
                if (Micron.U2NMMA30.clsTool.IsLimit(sLoc, ref fork))
                {
                    #region 極限儲位指定Fork
                    if (FunPickupForStockOut_Proc(batch_out, EquNo, (int)fork, ref sRemark)) return true;
                    else
                    {
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    #endregion 極限儲位指定Fork
                }
                else
                {
                    if (FunPickupForStockOut_Proc(batch_out, EquNo, ref sRemark)) return true;
                    else
                    {
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
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

        public static bool FunPickupProc_ForStockOut(CmdMstInfo batch_out, ref string sRemark)
        {
            try
            {
                if (batch_out.CmdSts != clsConstValue.CmdSts.strCmd_Initial && batch_out.CurLoc != LocationDef.Location.Teach.ToString())
                    return false;

                clsEnum.Fork fork = clsEnum.Fork.None;
                if (Micron.U2NMMA30.clsTool.IsLimit(batch_out.Loc, ref fork))
                {
                    #region 極限儲位指定Fork
                    if (FunPickupForStockOut_Proc(batch_out, (int)fork, ref sRemark)) return true;
                    else
                    {
                        if (sRemark != batch_out.Remark)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(batch_out.CmdSno, sRemark);
                        }

                        return false;
                    }
                    #endregion 極限儲位指定Fork
                }
                else
                {
                    if (FunPickupForStockOut_Proc_CmdMstInfo(batch_out, Convert.ToInt32(batch_out.EquNo), ref sRemark)) return true;
                    else
                    {
                        if (sRemark != batch_out.Remark)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(batch_out.CmdSno, sRemark);
                        }

                        return false;
                    }
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

        public static bool FunPickupForStockIn_Proc(DataGridViewRow row, Location Start, Location End)
        {
            try
            {
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                if (!clsMicronCV.GetConveyorController().IsConnected)
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                ConveyorInfo buffer = Micron.U2NMMA30.clsTool.GetBufferFromLocation(Start);
                if(buffer == null)
                {
                    sRemark = $"Error: 取得CV Buffer資訊失敗 => <DeviceID> {Start.DeviceId} <Location> {Start.LocationId}";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
                else
                {
                    var cv = clsMicronCV.GetConveyorController().GetBuffer(buffer.Index);
                    if (cv.Ready == (int)clsEnum.Ready.IN)
                    {
                        if (sCmdSno == cv.CommandID)
                        {
                            int fork = End.LocationId == LocationDef.Location.LeftFork.ToString() ? 1 : 2;
                            return clsDB_Proc.GetDB_Object().GetProcess().FunCVToFork_Proc(row, buffer, int.Parse(Start.DeviceId), fork);
                        }
                        else
                        {
                            sRemark = $"Error: {buffer.BufferName}的任務號不是{sCmdSno} => {cv.CommandID}";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        sRemark = $"Error: {buffer.BufferName}並非入庫Ready ({(int)clsEnum.Ready.IN})";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
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

        public static bool FunShelfToFork_NeedL2L_Proc(DataGridViewRow NeedL2L, ref string sRemark)
        {
            try
            {
                string sLoc = Convert.ToString(NeedL2L.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                string sCmdSno = Convert.ToString(NeedL2L.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sLocDD = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().GetLocDD(sLoc);
                string sRemark_Pre = Convert.ToString(NeedL2L.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                int EquNo = Convert.ToInt32(NeedL2L.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                string sBatchID = Convert.ToString(NeedL2L.Cells[ColumnDef.CMD_MST.BatchID.Index].Value);
                if (string.IsNullOrWhiteSpace(sLocDD))
                {
                    sRemark = "Error: 取得對照儲位失敗！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                bool IsEmpty = false; string BoxID_DD = "";
                int iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsEmpty(sLocDD, ref IsEmpty, ref BoxID_DD);
                if (iRet != DBResult.Success)
                {
                    sRemark = $"Error: 確認對照儲位{sLocDD}的狀態失敗！";
                    if (sRemark != sRemark_Pre)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }

                if (IsEmpty)
                {
                    #region 內儲位已空
                    if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(sCmdSno, clsEnum.NeedL2L.N)) return true;
                    else
                    {
                        sRemark = $"Error: 更新NeedShelfToShelf欄位失敗 => {clsEnum.NeedL2L.N.ToString()}";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    #endregion 內儲位已空
                }
                else
                {
                    CmdMstInfo cmd_DD = new CmdMstInfo();
                    iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                    if (iRet == DBResult.Success)
                    {
                        if (!string.IsNullOrWhiteSpace(sBatchID) && sBatchID == cmd_DD.BatchID &&
                            cmd_DD.CmdSts == clsConstValue.CmdSts.strCmd_Initial)
                        {
                            return FunPickupProc_ForStockOut(cmd_DD, ref sRemark);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(sBatchID) && !string.IsNullOrWhiteSpace(cmd_DD.BatchID) &&
                                sBatchID != cmd_DD.BatchID && cmd_DD.CmdSts == clsConstValue.CmdSts.strCmd_Initial)
                            {
                                return clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(cmd_DD.CmdMode, cmd_DD.BatchID);
                            }
                            else
                            {
                                sRemark = $"Error: 等候內儲位{sLocDD}的命令完成！";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                return false;
                            }
                        }
                    }
                    else if (iRet != DBResult.NoDataSelect)
                    {
                        sRemark = $"Error: 確認內儲位{sLocDD}的命令失敗！";
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        return false;
                    }
                    else
                    {   //上報Shelf Request
                        return clsDB_Proc.GetDB_Object().GetProcess().FunShelfRequest_Proc(sCmdSno, sLocDD, sRemark_Pre);
                    }
                }
            }
            catch (Exception ex)
            {
                sRemark = ex.Message;
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunShelfToFork_Proc(DataGridViewRow row)
        {
            try
            {
                string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                int iEquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                if (
                     sCmdMode == clsConstValue.CmdMode.StockOut ||
                     (sCmdMode == clsConstValue.CmdMode.L2L &&
                      iEquNo != Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc))
                                              )
                {
                    return FunPickupProc_ForStockOut(row, iEquNo, ref sRemark);
                }
                else
                {
                    clsEnum.Fork fork = clsEnum.Fork.None;
                    if (!Micron.U2NMMA30.clsTool.IsLimit(sLoc, ref fork))
                    {
                        var crane = clsMicronStocker.GetStockerById(iEquNo).GetCraneById(1);

                        if (
                             (crane.GetForkById(1).GetConfig().Enable &&
                              crane.GetForkById(2).GetConfig().Enable)
                             ||
                             (!crane.GetForkById(1).GetConfig().Enable &&
                              !crane.GetForkById(2).GetConfig().Enable)
                           )
                        {
                            if (!crane.GetForkById(1).HasCarrier)
                            {
                                fork = clsEnum.Fork.Left;
                            }
                            else fork = clsEnum.Fork.Right;
                        }
                        else if(crane.GetForkById(1).GetConfig().Enable) fork = clsEnum.Fork.Left;
                        else fork = clsEnum.Fork.Right;
                    }

                    return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, (int)fork);
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

        public static bool FunShelfToFork_Proc(DataGridViewRow row, string sLoc, string sNewLoc, int iEquNo)
        {
            clsEnum.Fork fork = clsEnum.Fork.None;
            if (Micron.U2NMMA30.clsTool.IsLimit(sLoc, ref fork))
            {
                return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, (int)fork);
            }
            else
            {
                if (Micron.U2NMMA30.clsTool.IsLimit(sNewLoc, ref fork))
                {
                    return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, (int)fork);
                }
                else
                {
                    var crane = clsMicronStocker.GetStockerById(iEquNo).GetCraneById(1);

                    if (
                        (crane.GetForkById(1).GetConfig().Enable &&
                         crane.GetForkById(2).GetConfig().Enable)
                        ||
                        (!crane.GetForkById(1).GetConfig().Enable &&
                         !crane.GetForkById(2).GetConfig().Enable)
                       )
                    {
                        if (!crane.GetForkById(1).HasCarrier)
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, 1);
                        }
                        else
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, 2);
                        }
                    }
                    else if (crane.GetForkById(1).GetConfig().Enable)
                    {
                        return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, 1);
                    }
                    else
                    {
                        return clsDB_Proc.GetDB_Object().GetProcess().FunShelfToFork_Proc(row, 2);
                    }
                }
            }
        }
    }
}
*/