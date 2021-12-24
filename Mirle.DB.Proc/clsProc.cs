using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Mirle.DB.Fun;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.Structure;
using Mirle.Structure.Info;
using System.Linq;
//using Mirle.WebAPI.U2NMMA30.ReportInfo;
//using Mirle.Micron.U2NMMA30;
//using Mirle.Def.U2NMMA30;
//using Mirle.Grid.U2NMMA30;



namespace Mirle.DB.Proc
{
    public class clsProc
    {
        private clsPortDef PortDef = new clsPortDef();
        private Fun.clsCmd_Mst CMD_MST = new Fun.clsCmd_Mst();
        private Fun.clsTask TaskTable = new Fun.clsTask();
        private Fun.clsSno SNO = new Fun.clsSno();
        private Fun.clsLocMst LocMst = new Fun.clsLocMst();
        private Fun.clsProc proc;
        private Fun.clsL2LCount L2LCount = new Fun.clsL2LCount();
        private Fun.clsAlarmData alarmData = new Fun.clsAlarmData();
        private Fun.clsCmd_Mst_His CMD_MST_HIS = new Fun.clsCmd_Mst_His();
        private Fun.clsUnitStsLog unitStsLog = new Fun.clsUnitStsLog();

        public List<Element_Port>[] GetLstPort()
        {
            return PortDef.GetLstPort();
        }

        private clsDbConfig _config = new clsDbConfig();
        private clsDbConfig _config_WMS = new clsDbConfig();
        private clsDbConfig _config_Sqlite = new clsDbConfig();
        private OEEParamConfig _config_OEEParam = new OEEParamConfig();
        public clsProc(clsDbConfig config, clsDbConfig config_WMS, clsDbConfig config_Sqlite, OEEParamConfig config_OEEParam)
        {
            _config = config;
            _config_WMS = config_WMS;
            _config_Sqlite = config_Sqlite;
            _config_OEEParam = config_OEEParam;
            proc = new Fun.clsProc(_config_WMS);
        }

        public Fun.clsProc GetFunProcess()
        {
            return proc;
        }

        public bool GetDevicePortProc()
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (PortDef.FunDevice(db))
                        {
                            PortDef.FunGetAllPort(db);
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunInsCmd_ForL2L(CmdMstInfo cmd, ref string sRemark)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        iRet = L2LCount.CheckHasData(cmd.BoxID, ref sRemark, db);
                        if (iRet == DBResult.Exception) return false;
                        else
                        {
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                sRemark = "Begin失敗！";
                                return false;
                            }

                            if (!CMD_MST.FunInsCmdMst(cmd, ref sRemark, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet == DBResult.Success)
                            {
                                if (!L2LCount.FunUpdL2LCount(cmd.BoxID, ref sRemark, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }
                            else
                            {
                                if (!L2LCount.FunInsL2LCount(cmd.BoxID, ref sRemark, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                    }
                    else
                    {
                        sRemark = "NG:資料庫開啟失敗！";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunCarrierTransferCancel(string jobId, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand_byJobId(jobId, ref cmd, db))
                        {
                            if (cmd.CmdSts == clsConstValue.CmdSts.strCmd_Running)
                            {
                                strEM = "Error: 命令已開始執行，無法取消！";
                                return false;
                            }

                            int iRet_Task = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            int iRet_Teach = LocMst.GetTeachLoc_byBoxID(cmd.BoxID, db);
                            if (iRet_Teach == DBResult.Exception)
                            {
                                strEM = "取得校正儲位資料失敗！";
                                return false;
                            }

                            if (iRet_Task == DBResult.Success) TaskTable.FunInsertHisTask(cmd.CmdSno, db);

                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Cancel, "WMS命令取消", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (iRet_Teach == DBResult.Success)
                            {
                                if (!LocMst.FunClearTeachLoc_byBoxID(cmd.BoxID, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<JobId> {jobId} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        #region Mark
        /*
        public bool FunShelfToFork_BatchProc(TaskDTO[] taskInfo, ref string sRemark)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return proc.FunShelfToFork_BatchProc(taskInfo, ref sRemark, db);
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunStockIn_L2L_Finish_Proc(string sCmdSno)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            iRet = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);

                            string sCurLoc = "";
                            if (cmd.CmdMode == clsConstValue.CmdMode.L2L) sCurLoc = cmd.NewLoc;
                            else sCurLoc = cmd.Loc;

                            string sRemark = "";
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                sRemark = "Error: Begin失敗！";
                                if (sRemark != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                                }

                                return false;
                            }

                            sRemark = "入庫命令已完成";
                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Finished, clsEnum.Cmd_Abnormal.NA, sRemark, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet == DBResult.Success)
                            {
                                if (!TaskTable.FunInsertHisTask(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }

                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (cmd.CmdMode == clsConstValue.CmdMode.StockIn)
                            {
                                PutAwayCompleteInfo info = new PutAwayCompleteInfo
                                {
                                    carrierId = cmd.BoxID,
                                    isComplete = clsEnum.WmsApi.IsComplete.Y.ToString(),
                                    jobId = cmd.JobID,
                                    shelfId = sCurLoc
                                };

                                if (!clsWmsApi.GetApiProcess().GetPutAwayComplete().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }
                            else
                            {
                                ShelfCompleteInfo info = new ShelfCompleteInfo
                                {
                                    carrierId = cmd.BoxID,
                                    jobId = cmd.JobID,
                                    shelfId = sCurLoc
                                };

                                if (!clsWmsApi.GetApiProcess().GetShelfComplete().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunL2LToTeach_Finish_Proc(string sCmdSno)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            string sRemark = "";
                            int iStockerID = Micron.U2NMMA30.clsTool.funGetEquNoByLoc(cmd.NewLoc);
                            bool IsTeach = false;
                            string sNewLoc_Teach = Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(cmd.NewLoc);
                            iRet = LocMst.CheckIsTeach(iStockerID.ToString(), sNewLoc_Teach, ref IsTeach, db);
                            if (iRet == DBResult.Exception)
                            {
                                sRemark = $"Error: 確認{cmd.NewLoc}是否是校正儲位失敗！";
                                if (sRemark != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }

                            if (!IsTeach)
                            {
                                sRemark = $"Error: {cmd.NewLoc}並非是校正儲位！";
                                if (sRemark != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }

                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                sRemark = "Error: Begin失敗！";
                                if (sRemark != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmd_ForTeachCmd(sCmdSno, cmd.NewLoc, cmd.Loc, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (!LocMst.FunUpdLocSts(iStockerID.ToString(), sNewLoc_Teach, clsEnum.LocSts.O, cmd.BoxID, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        
        public bool FunDepositToShelf_Proc(int StockerID, int Fork, string sCmdMode)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        iRet = TaskTable.CheckHasTaskCmd(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, db);
                        if (iRet == DBResult.Success)
                        {
                            string sRemark = "Error: 該命令有設備命令正在執行！";
                            if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                            {
                                CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (iRet != DBResult.NoDataSelect)
                            {
                                string sRemark = "Error: 檢查Task命令失敗！";
                                if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                                {
                                    CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                                }

                                return false;
                            }
                        }

                        if (TaskTable.funCheckTaskCmdRepeat(StockerID.ToString(), Fork, db))
                        {
                            string sRemark = $"Error: Stocker{StockerID}的Fork{Fork}還有命令正在執行！";
                            if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                            {
                                CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            string Loc_Task = ""; string sRemark = "";
                            string sRemark_Pre = clsMicronStocker.GetCommand(StockerID, Fork).Remark;
                            string sCmdSno = clsMicronStocker.GetCommand(StockerID, Fork).CommandID;
                            if (sCmdMode == clsConstValue.CmdMode.StockIn) Loc_Task = clsMicronStocker.GetCommand(StockerID, Fork).Loc;
                            else Loc_Task = clsMicronStocker.GetCommand(StockerID, Fork).NewLoc;

                            if (!Micron.U2NMMA30.clsTool.CheckForkCanDo(Loc_Task, StockerID, Fork))
                            {
                                return proc.FunChangeHand_Proc(clsMicronStocker.GetCommand(StockerID, Fork), StockerID, Fork, db);
                            }
                            else
                            {
                                string TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                                if (string.IsNullOrWhiteSpace(TaskNo))
                                {
                                    sRemark = "Error: 取得TaskNo失敗！";
                                    if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                                    {
                                        CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                                    }

                                    return false;
                                }
                                else
                                {
                                    string strEM = "";
                                    if (TaskTable.FunInsertTaskCmd(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, TaskNo, StockerID.ToString(),
                                         clsEnum.TaskMode.Deposit, clsMicronStocker.GetShelfIdByFork(Fork), Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(Loc_Task),
                                         ref strEM, clsMicronStocker.GetCommand(StockerID, Fork).Priority,
                                         clsMicronStocker.GetCommand(StockerID, Fork).CarrierID, clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).Speed, Fork, db))
                                    {
                                        sRemark = $"下達Stocker{StockerID}置物命令";
                                        if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                                        {
                                            CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        sRemark = $"Error: {strEM}";
                                        if (sRemark != clsMicronStocker.GetCommand(StockerID, Fork).Remark)
                                        {
                                            CMD_MST.FunUpdateRemark(clsMicronStocker.GetCommand(StockerID, Fork).CommandID, sRemark, db);
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        
        public bool FunDepositToShelf_Proc(DataGridViewRow row)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                        string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                        string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                        string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                        string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                        string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                        string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                        string CarrierID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                        int Pry = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.PRT.Index].Value);
                        int Fork = sCurLoc == LocationDef.Location.LeftFork.ToString() ? 1 : 2;

                        iRet = TaskTable.CheckHasTaskCmd(sCmdSno, db);
                        if (iRet == DBResult.Success)
                        {
                            string sRemark = "Error: 該命令有設備命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (iRet != DBResult.NoDataSelect)
                            {
                                string sRemark = "Error: 檢查Task命令失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                        }

                        if (TaskTable.funCheckTaskCmdRepeat(sCurDeviceID, Fork, db))
                        {
                            string sRemark = $"Error: Stocker{sCurDeviceID}的Fork{Fork}還有命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            string Loc_Task = "";
                            if (sCmdMode == clsConstValue.CmdMode.StockIn) Loc_Task = sLoc;
                            else Loc_Task = sNewLoc;

                            if (!Micron.U2NMMA30.clsTool.CheckForkCanDo(Loc_Task, int.Parse(sCurDeviceID), Fork))
                            {
                                TransferBatch cmd = new TransferBatch
                                {
                                    CarrierID = CarrierID,
                                    CmdMode = sCmdMode,
                                    CommandID = sCmdSno,
                                    Loc = sLoc,
                                    StnNo = Convert.ToString(row.Cells[ColumnDef.CMD_MST.StnNo.Index].Value),
                                    NewLoc = sNewLoc,
                                    Priority = Pry,
                                    Remark = sRemark_Pre,
                                    BatchID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BatchID.Index].Value)
                                };

                                if (int.Parse(sCurDeviceID) == 4)
                                {   //Single Deep無校正儲位
                                    return proc.FunChangeHand_Proc(cmd, int.Parse(sCurDeviceID), Fork, db);
                                }
                                else
                                {   //置物進校正儲位流程
                                    if (proc.FunCreateForkToTeach_Proc(cmd, int.Parse(sCurDeviceID), Fork, db)) return true;
                                    else return proc.FunChangeHand_Proc(cmd, int.Parse(sCurDeviceID), Fork, db);
                                }
                            }
                            else
                            {
                                string TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                                if (string.IsNullOrWhiteSpace(TaskNo))
                                {
                                    string sRemark = "Error: 取得TaskNo失敗！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }
                                else
                                {
                                    string strEM = "";

                                    if (TaskTable.FunInsertTaskCmd(sCmdSno, TaskNo, sCurDeviceID,
                                         clsEnum.TaskMode.Deposit, clsMicronStocker.GetShelfIdByFork(Fork), Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(Loc_Task),
                                         ref strEM, Pry, CarrierID, clsMicronStocker.GetStockerById(int.Parse(sCurDeviceID)).GetCraneById(1).Speed, Fork, db))
                                    {
                                        string sRemark = $"下達Stocker{sCurDeviceID}置物命令";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        string sRemark = $"Error: {strEM}";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDepositToCV_Proc(DataGridViewRow row, ConveyorInfo buffer)
        {
            try
            {
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (!clsMicronCV.GetConveyorController().IsConnected)
                        {
                            sRemark = "Error: CV PLC連線異常！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).Ready != (int)clsEnum.Ready.OUT)
                        {
                            sRemark = $"Error: {buffer.BufferName}並非出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                        string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                        string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                        string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                        int fork = sCurLoc == LocationDef.Location.LeftFork.ToString() ? 1 : 2;
                        string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);

                        iRet = TaskTable.CheckHasTaskCmd(sCmdSno, db);
                        if (iRet == DBResult.Success)
                        {
                            sRemark = "Error: 該命令有設備命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (iRet != DBResult.NoDataSelect)
                            {
                                sRemark = "Error: 檢查Task命令失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                        }

                        if (TaskTable.funCheckTaskCmdRepeat(sCurDeviceID, fork, db))
                        {
                            sRemark = $"Error: Stocker{sCurDeviceID}的Fork{fork}還有命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sStnNo_List = Convert.ToString(row.Cells[ColumnDef.CMD_MST.StnNo.Index].Value);
                        int Pry = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.PRT.Index].Value);
                        string sBoxID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                        string BackupPortID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BackupPortId.Index].Value);
                        bool bGetPath = false; bool bIsChangeBackupPort = false;
                        string sNewStnList = "";
                        int Path = 0; string[] sStnNo = new string[0];
                        string sticketId = Convert.ToString(row.Cells[ColumnDef.CMD_MST.TicketId.Index].Value); ;
                        string smallticketId = "";
                        switch (sCmdMode)
                        {
                            case clsConstValue.CmdMode.StockIn:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sLoc));
                                break;
                            case clsConstValue.CmdMode.L2L:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc));
                                break;
                            case clsConstValue.CmdMode.S2S:
                                if (sBoxID.Substring(0, 6) == clsConstValue.CarrierNoCmd)
                                {
                                    Path = clsMicronCV.GetPathByStockerID(Convert.ToInt32(sCurDeviceID));
                                    break;
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(sStnNo_List))
                                    {
                                        sRemark = "Error: 站號欄位為空，請檢查！";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                        }

                                        return false;
                                    }
                                    Path = clsMicronCV.GetPathByStnNo(sStnNo_List);
                                    break;
                                }
                            case clsConstValue.CmdMode.StockOut:
                                sStnNo = sStnNo_List.Split(',');
                                if (sStnNo == null || sStnNo.Length == 0)
                                {
                                    sRemark = "Error: 站號欄位為空，請檢查！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }
                                //優先全為1時，就不去判斷訂單號
                                if (Pry == 1)
                                {
                                    bGetPath = clsMicronCV.GetPathByStockOut(sStnNo_List, BackupPortID, "", ref bIsChangeBackupPort, ref sNewStnList, ref Path);
                                    if (!bGetPath)
                                        Path = clsMicronCV.GetPathByStnNo(sStnNo[0]);
                                    break;
                                }
                                else
                                {
                                    CMD_MST.FunGetSmallTicketId(sStnNo[0], sticketId, ref smallticketId, db);
                                    if (string.IsNullOrWhiteSpace(sticketId) || sticketId == smallticketId)
                                    {
                                        bGetPath = clsMicronCV.GetPathByStockOut(sStnNo_List, BackupPortID, "", ref bIsChangeBackupPort, ref sNewStnList, ref Path);
                                        if (!bGetPath)
                                            Path = clsMicronCV.GetPathByStnNo(sStnNo[0]);
                                        break;
                                    }
                                    else
                                    {
                                        int StockerId = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                                        Path = clsMicronCV.GetPathByStockerID(StockerId);
                                        break;
                                    }
                                }

                            default:
                                sRemark = $"Error: 該模式不存在 => {sCmdMode}";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                        }

                        string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                        if (string.IsNullOrWhiteSpace(sTaskNo))
                        {
                            sRemark = "Error: 取得TaskNo失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            sRemark = "Error: Begin失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (sCmdMode == clsConstValue.CmdMode.StockOut && bIsChangeBackupPort)
                        {
                            sRemark = $"下達Stocker{sCurDeviceID}至站口的置物命令";
                            if (!CMD_MST.FunUpdateStnNo(sCmdSno, sNewStnList, sRemark, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }
                        }

                        if (!TaskTable.FunInsertTaskCmd(sCmdSno, sTaskNo, sCurDeviceID, clsEnum.TaskMode.Deposit,
                            clsMicronStocker.GetShelfIdByFork(fork), buffer.StkPortID.ToString(), ref sRemark, Pry, sBoxID, clsMicronStocker.GetStockerById(int.Parse(sCurDeviceID)).GetCraneById(1).Speed,
                            fork, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAsync(sCmdSno, int.Parse(sCmdMode), Path).Result)
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);

                            sRemark = $"Error: CV PLC填值失敗 => {buffer.BufferName} <Command> {sCmdSno} <CmdMode> {sCmdMode} <Path> {Path}";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        return true;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDepositToCV_Proc(TransferBatch cmd, int StockerID, int fork, ConveyorInfo buffer)
        {
            try
            {
                string sCmdSno = cmd.CommandID;
                string sRemark_Pre = cmd.Remark;
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (!clsMicronCV.GetConveyorController().IsConnected)
                        {
                            sRemark = "Error: CV PLC連線異常！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).Ready != (int)clsEnum.Ready.OUT)
                        {
                            sRemark = $"Error: {buffer.BufferName}並非出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sCmdMode = cmd.CmdMode;
                        string sLoc = cmd.Loc;
                        string sNewLoc = cmd.NewLoc;

                        iRet = TaskTable.CheckHasTaskCmd(sCmdSno, db);
                        if (iRet == DBResult.Success)
                        {
                            sRemark = "Error: 該命令有設備命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (iRet != DBResult.NoDataSelect)
                            {
                                sRemark = "Error: 檢查Task命令失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                        }

                        if (TaskTable.funCheckTaskCmdRepeat(StockerID.ToString(), fork, db))
                        {
                            sRemark = $"Error: Stocker{StockerID}的Fork{fork}還有命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sStnNo_List = cmd.StnNo;
                        int Pry = cmd.Priority;
                        string sBoxID = cmd.CarrierID;
                        bool bGetPath = false; bool bIsChangeBackupPort = false;
                        string sNewStnList = "";
                        int Path = 0; string[] sStnNo = new string[0];
                        string sticketId = cmd.TicketId;
                        string smallticketId = "";
                        switch (sCmdMode)
                        {
                            case clsConstValue.CmdMode.StockIn:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sLoc));
                                break;
                            case clsConstValue.CmdMode.L2L:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc));
                                break;
                            case clsConstValue.CmdMode.S2S:
                                if (sBoxID.Substring(0, 6) == clsConstValue.CarrierNoCmd)
                                {
                                    Path = clsMicronCV.GetPathByStockerID(Convert.ToInt32(StockerID));
                                    break;
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(sStnNo_List))
                                    {
                                        sRemark = "Error: 站號欄位為空，請檢查！";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                        }

                                        return false;
                                    }
                                    Path = clsMicronCV.GetPathByStnNo(sStnNo_List);
                                    break;
                                }
                            case clsConstValue.CmdMode.StockOut:
                                sStnNo = sStnNo_List.Split(',');
                                if (sStnNo == null || sStnNo.Length == 0)
                                {
                                    sRemark = "Error: 站號欄位為空，請檢查！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }

                                if (Pry == 1)
                                {
                                    bGetPath = clsMicronCV.GetPathByStockOut(sStnNo_List, cmd.BackupPortID, "", ref bIsChangeBackupPort, ref sNewStnList, ref Path);
                                    if (!bGetPath)
                                        Path = clsMicronCV.GetPathByStnNo(sStnNo[0]);
                                    break;
                                }
                                else
                                {
                                    CMD_MST.FunGetSmallTicketId(sStnNo[0], sticketId, ref smallticketId, db);
                                    if (string.IsNullOrWhiteSpace(sticketId) || sticketId == smallticketId)
                                    {
                                        bGetPath = clsMicronCV.GetPathByStockOut(sStnNo_List, cmd.BackupPortID, "", ref bIsChangeBackupPort, ref sNewStnList, ref Path);
                                        if (!bGetPath)
                                            Path = clsMicronCV.GetPathByStnNo(sStnNo[0]);
                                        break;
                                    }
                                    else
                                    {
                                        Path = clsMicronCV.GetPathByStockerID(StockerID);
                                        break;
                                    }
                                }


                            default:
                                sRemark = $"Error: 該模式不存在 => {sCmdMode}";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                        }

                        string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                        if (string.IsNullOrWhiteSpace(sTaskNo))
                        {
                            sRemark = "Error: 取得TaskNo失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            sRemark = "Error: Begin失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (sCmdMode == clsConstValue.CmdMode.StockOut && bIsChangeBackupPort)
                        {
                            sRemark = $"下達Stocker{StockerID}至站口的置物命令";
                            if (!CMD_MST.FunUpdateStnNo(sCmdSno, sNewStnList, sRemark, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }
                        }

                        if (!TaskTable.FunInsertTaskCmd(sCmdSno, sTaskNo, StockerID.ToString(), clsEnum.TaskMode.Deposit,
                            clsMicronStocker.GetShelfIdByFork(fork), buffer.StkPortID.ToString(), ref sRemark, Pry, sBoxID, clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).Speed,
                            fork, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAsync(sCmdSno, int.Parse(sCmdMode), Path).Result)
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);

                            sRemark = $"Error: CV PLC填值失敗 => {buffer.BufferName} <Command> {sCmdSno} <CmdMode> {sCmdMode} <Path> {Path}";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        return true;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunShelfToFork_Proc(DataGridViewRow row, int fork)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return proc.FunShelfToFork_Proc(row, fork, db);
                    }
                    else
                    {
                        string sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunShelfToFork_Proc(CmdMstInfo row, int fork)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return proc.FunShelfToFork_Proc(row, fork, db);
                    }
                    else
                    {
                        string sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 從校正儲位取物流程
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool FunPickupFromTeach_Proc(DataGridViewRow row)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                        string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                        string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                        string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                        string sBoxID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                        string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                        string sEquNo = Convert.ToString(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                        string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                        string sRemark = "";

                        string sSource = LocMst.GetLoc(sBoxID, db);
                        if (string.IsNullOrWhiteSpace(sSource))
                        {
                            sRemark = "Error: 取得校正儲位編號失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            string sLoc_Check = "";
                            if (sCmdMode == clsConstValue.CmdMode.StockIn) sLoc_Check = sLoc;
                            else sLoc_Check = sNewLoc;

                            clsEnum.Fork fork = clsEnum.Fork.None;
                            bool bToCV = false;
                            var crane = clsMicronStocker.GetStockerById(int.Parse(sCurDeviceID)).GetCraneById(1);
                            if (
                                (sCmdMode == clsConstValue.CmdMode.StockIn &&
                                 sCurDeviceID == sEquNo)
                                ||
                                (sCmdMode == clsConstValue.CmdMode.L2L &&
                                 sCurDeviceID == Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc).ToString())
                              )
                            {
                                if (!Micron.U2NMMA30.clsTool.IsLimit(sLoc_Check, ref fork))
                                {
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
                                    else if (crane.GetForkById(1).GetConfig().Enable)
                                        fork = clsEnum.Fork.Left;
                                    else fork = clsEnum.Fork.Right;
                                }

                                bToCV = false;
                            }
                            else
                            {
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
                                else if (crane.GetForkById(1).GetConfig().Enable)
                                    fork = clsEnum.Fork.Left;
                                else fork = clsEnum.Fork.Right;

                                bToCV = true;
                            }

                            if (crane.GetForkById((int)fork).HasCarrier)
                            {
                                sRemark = $"Error: Stocker{sCurDeviceID}的Fork{(int)fork}有物，請檢查！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                            else
                            {
                                bool bDoIt = false;
                                if (int.Parse(sCurDeviceID) == 4) bDoIt = true;    //Single Deep直接做
                                else
                                {
                                    if (bToCV)
                                    {
                                        if (!clsMicronCV.GetConveyorController().IsConnected)
                                        {
                                            sRemark = "Error: CV PLC連線異常！";
                                            if (sRemark != sRemark_Pre)
                                            {
                                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                            }

                                            bDoIt = false;
                                        }
                                        else
                                        {
                                            ConveyorInfo buffer = new ConveyorInfo();
                                            bool checkBuffer = clsMicronCV.GetStockOutBuffer(int.Parse(sCurDeviceID), (int)fork, ref buffer, ref sRemark);
                                            if (!checkBuffer)
                                            {
                                                if (sRemark != sRemark_Pre)
                                                {
                                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                                }

                                                bDoIt = false;
                                            }
                                            else bDoIt = true;
                                        }
                                    }
                                    else bDoIt = true;
                                }

                                if (bDoIt) return proc.FunCreateTeachToFork_Proc(row, sSource, (int)fork, db);
                                else return false;
                            }
                        }
                    }
                    else
                    {
                        string sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
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

        public bool FunCVToFork_Proc(DataGridViewRow row, ConveyorInfo buffer, int StockerID, int fork)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return proc.FunCVToFork_Proc(row, buffer, StockerID, fork, db);
                    }
                    else
                    {
                        string sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunShelfToFork_Proc(IEnumerable<DataGridViewRow> obj_Batch, int fork)
        {
            try
            {
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        bool bCheck = false; string sLoc = "";
                        foreach (var row in obj_Batch)
                        {
                            sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                            int EquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                            if (!Micron.U2NMMA30.clsTool.CheckForkCanDo(sLoc, EquNo, fork)) continue;

                            iRet = proc.GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sLoc, ref bCheck);
                            if (iRet == DBResult.Success)
                            {
                                if (bCheck) break;
                            }
                            else return false;
                        }

                        if (bCheck)
                        {
                            var obj = obj_Batch.Where(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.Loc.Index].Value) == sLoc);
                            if (obj == null || obj.Count() == 0) return false;
                            foreach (var row in obj)
                            {
                                return proc.FunShelfToFork_Proc(row, fork, db);
                            }
                        }
                        else
                        {
                            foreach (var row in obj_Batch)
                            {
                                if (proc.FunShelfToFork_Proc(row, fork, db)) return true;
                                else continue;
                            }
                        }

                        return false;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDepositToCV_Proc_addCancelBatch(TransferBatch cmd, int StockerID, int fork, ConveyorInfo buffer)
        {
            try
            {
                string sCmdSno = cmd.CommandID;
                string sRemark_Pre = cmd.Remark;
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (!clsMicronCV.GetConveyorController().IsConnected)
                        {
                            sRemark = "Error: CV PLC連線異常！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).Ready != (int)clsEnum.Ready.OUT)
                        {
                            sRemark = $"Error: {buffer.BufferName}並非出庫Ready！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sCmdMode = cmd.CmdMode;
                        string sLoc = cmd.Loc;
                        string sNewLoc = cmd.NewLoc;

                        iRet = TaskTable.CheckHasTaskCmd(sCmdSno, db);
                        if (iRet == DBResult.Success)
                        {
                            sRemark = "Error: 該命令有設備命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (iRet != DBResult.NoDataSelect)
                            {
                                sRemark = "Error: 檢查Task命令失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                        }

                        if (TaskTable.funCheckTaskCmdRepeat(StockerID.ToString(), fork, db))
                        {
                            sRemark = $"Error: Stocker{StockerID}的Fork{fork}還有命令正在執行！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        string sStnNo_List = cmd.StnNo;
                        int Pry = cmd.Priority;
                        string sBoxID = cmd.CarrierID;
                        bool bGetPath = false; bool bIsChangeBackupPort = false;
                        string sNewStnList = "";
                        int Path = 0; string[] sStnNo = new string[0];
                        switch (sCmdMode)
                        {
                            case clsConstValue.CmdMode.StockIn:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sLoc));
                                break;
                            case clsConstValue.CmdMode.L2L:
                                Path = clsMicronCV.GetPathByStockerID(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc));
                                break;
                            case clsConstValue.CmdMode.StockOut:
                                sStnNo = sStnNo_List.Split(',');
                                if (sStnNo == null || sStnNo.Length == 0)
                                {
                                    sRemark = "Error: 站號欄位為空，請檢查！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }

                                bGetPath = clsMicronCV.GetPathByStockOut(sStnNo_List, cmd.BackupPortID, "", ref bIsChangeBackupPort,
                                   ref sNewStnList, ref Path);
                                if (!bGetPath)
                                    Path = clsMicronCV.GetPathByStnNo(sStnNo[0]);
                                break;
                            default:
                                sRemark = $"Error: 該模式不存在 => {sCmdMode}";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                        }

                        string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                        if (string.IsNullOrWhiteSpace(sTaskNo))
                        {
                            sRemark = "Error: 取得TaskNo失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            sRemark = "Error: Begin失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (!CMD_MST.FunCancelBatch(sCmdMode, cmd.BatchID, db))
                        {
                            sRemark = "Error: 取消Batch失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        //if (sCmdMode == clsConstValue.CmdMode.StockOut)
                        //{
                        //    string sStnNo_New = "";
                        //    if (sStnNo.Length == 1) sStnNo_New = "";
                        //    else
                        //    {
                        //        for (int i = 1; i < sStnNo.Length; i++)
                        //        {
                        //            if (i == 1)
                        //            {
                        //                sStnNo_New += sStnNo[i];
                        //            }
                        //            else
                        //            {
                        //                sStnNo_New += $",{sStnNo[i]}";
                        //            }
                        //        }
                        //    }

                        //    sRemark = $"下達Stocker{StockerID}至站口的置物命令";
                        //    if (!CMD_MST.FunUpdateStnNo(sCmdSno, sStnNo_New, sRemark, db))
                        //    {
                        //        db.TransactionCtrl(TransactionTypes.Rollback);
                        //        return false;
                        //    }
                        //}

                        if (!TaskTable.FunInsertTaskCmd(sCmdSno, sTaskNo, StockerID.ToString(), clsEnum.TaskMode.Deposit,
                            clsMicronStocker.GetShelfIdByFork(fork), buffer.StkPortID.ToString(), ref sRemark, Pry, sBoxID, clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).Speed,
                            fork, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAsync(sCmdSno, int.Parse(sCmdMode), Path).Result)
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);

                            sRemark = $"Error: CV PLC填值失敗 => {buffer.BufferName} <Command> {sCmdSno} <CmdMode> {sCmdMode} <Path> {Path}";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);

                        if (sCmdMode == clsConstValue.CmdMode.StockOut && bIsChangeBackupPort)
                        {
                            CMD_MST.FunUpdateStnNo(sCmdSno, sNewStnList, sRemark, db);
                        }

                        return true;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunStockInWriPlc_Proc(DataGridViewRow row)
        {
            try
            {
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                        int iEquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                        string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                        string sStnNo = Convert.ToString(row.Cells[ColumnDef.CMD_MST.StnNo.Index].Value);
                        string sBoxID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                        string sJobID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.JobID.Index].Value);
                        string smanualStockIn = Convert.ToString(row.Cells[ColumnDef.CMD_MST.ManualStockIn.Index].Value);

                        ConveyorInfo buffer = clsMicronCV.GetBufferByStnNo(sStnNo);
                        if (buffer == null)
                        {
                            sRemark = "Error: 取得CV Buffer資料失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            var cv = clsMicronCV.GetConveyorController().GetBuffer(buffer.Index);
                            if (cv.Ready != (int)clsEnum.Ready.IN)
                            {
                                sRemark = $"Error: {buffer.BufferName}並非入庫Ready！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }

                            if (smanualStockIn != "Y")
                            {
                                if (sBoxID != cv.GetTrayID)
                                {
                                    sRemark = $"Error: <{buffer.BufferName}> 該命令的Bcr資料與現場不符！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }
                            }


                            int Path = clsMicronCV.GetPathByStockerID(iEquNo);
                            if (Path == 0)
                            {
                                sRemark = "Error: 取得CV路徑失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                }

                                return false;
                            }
                            else
                            {
                                sRemark = $"命令寫入PLC: <Buffer> {buffer.BufferName}";

                                if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                                {
                                    sRemark = "Error: Begin失敗！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    return false;
                                }

                                if (!CMD_MST.FunUpdateCmdSts(sCmdSno, clsConstValue.CmdSts.strCmd_Running, sRemark, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }

                                if (!cv.WriteCommandAsync(sCmdSno, 1, Path).Result)
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }

                                db.TransactionCtrl(TransactionTypes.Commit);

                                PositionReportInfo info = new PositionReportInfo
                                {
                                    carrierId = sBoxID,
                                    jobId = sJobID,
                                    location = "CV"
                                };

                                clsWmsApi.GetApiProcess().GetPositionReport().FunReport(info);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunWriPlcToNextStn_Proc(ConveyorInfo buffer, int Path, CmdMstInfo cmd, string StnNo_New)
        {
            try
            {
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        sRemark = $"命令寫入PLC: <Buffer> {buffer.BufferName}";
                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            sRemark = "Error: Begin失敗！";
                            if (sRemark != cmd.Remark)
                            {
                                CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (!CMD_MST.FunUpdateStnNo(cmd.CmdSno, StnNo_New, sRemark, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAsync(cmd.CmdSno, int.Parse(cmd.CmdMode), Path).Result)
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        return true;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunPutawayCheck_Proc(CmdMstInfo cmd, ConveyorInfo buffer, string StnNo_New)
        {
            try
            {
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var info = clsWmsApi.GetPutawayCheckInfo(buffer);
                        if (CMD_MST.FunUpdateStnNo(cmd.CmdSno, StnNo_New, "", db))
                        {
                            if (clsWmsApi.GetApiProcess().GetPutawayCheck().FunReport(info))
                            {
                                if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).SetReadReq().Result)
                                {
                                    clsWriLog.Log.FunWriTraceLog_CV($"NG: <Buffer> {buffer.BufferName} <Carrier> {info.carrierId} => 告知CV已收到條碼訊息失敗！");
                                    return false;
                                }
                            }
                            else return false;
                        }
                        else return false;

                        return true;
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunShelfRequest_Proc(string sCmdSno, string sLocDD, string sRemark_Pre)
        {
            try
            {
                string sRemark = "";
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        ShelfRequestInfo info = new ShelfRequestInfo
                        {
                            fromShelfId = sLocDD,
                            toShelfId = sLocDD
                        };

                        if (clsWmsApi.GetApiProcess().GetShelfRequest().FunReport(info))
                        {
                            sRemark = $"內儲位{sLocDD} => 上報Shelf Request至WMS";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }
                            return true;
                        }
                        else
                        {
                            sRemark = $"Error: 內儲位{sLocDD} => 上報Shelf Request至WMS失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                            }

                            return false;
                        }
                    }
                    else
                    {
                        sRemark = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunManualRepeatCmd(string sCmdSno, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            int iRet_Task = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            if (iRet_Task == DBResult.Success) TaskTable.FunInsertHisTask(cmd.CmdSno, db);
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Initial, "WCS手動重新執行命令", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<CmdSno> {sCmdSno} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunManualRepeatCmd(string sCmdSno, string sStnNo, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            int iRet_Task = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            if (iRet_Task == DBResult.Success) TaskTable.FunInsertHisTask(cmd.CmdSno, db);
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Initial, sStnNo, "WCS手動重新執行命令", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<CmdSno> {sCmdSno} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunManualCommandComplete(string sCmdSno, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            int iRet_Task = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            int iRet_Teach = LocMst.GetTeachLoc_byBoxID(cmd.BoxID, db);
                            if (iRet_Teach == DBResult.Exception)
                            {
                                strEM = "取得校正儲位資料失敗！";
                                return false;
                            }

                            if (iRet_Task == DBResult.Success) TaskTable.FunInsertHisTask(cmd.CmdSno, db);
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Finished, "WCS命令手動完成", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (iRet_Teach == DBResult.Success)
                            {
                                if (!LocMst.FunClearTeachLoc_byBoxID(cmd.BoxID, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (cmd.CmdMode == clsConstValue.CmdMode.StockIn)
                            {
                                PutAwayCompleteInfo info = new PutAwayCompleteInfo
                                {
                                    carrierId = cmd.BoxID,
                                    isComplete = clsEnum.WmsApi.IsComplete.Y.ToString(),
                                    jobId = cmd.JobID,
                                    shelfId = cmd.Loc
                                };

                                if (!clsWmsApi.GetApiProcess().GetPutAwayComplete().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }
                            else if (cmd.CmdMode == clsConstValue.CmdMode.L2L)
                            {
                                ShelfCompleteInfo info = new ShelfCompleteInfo
                                {
                                    carrierId = cmd.BoxID,
                                    jobId = cmd.JobID,
                                    shelfId = cmd.NewLoc
                                };

                                if (!clsWmsApi.GetApiProcess().GetShelfComplete().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }
                            else
                            {
                                string sStnNo = "";
                                if (string.IsNullOrWhiteSpace(cmd.StnNo)) sStnNo = ConveyorDef.A1_41.StnNo;
                                else sStnNo = cmd.StnNo;

                                RetrieveCompleteInfo info = new RetrieveCompleteInfo
                                {
                                    carrierId = cmd.BoxID,
                                    isComplete = clsEnum.WmsApi.IsComplete.Y.ToString(),
                                    jobId = cmd.JobID,
                                    portId = sStnNo
                                };

                                if (!clsWmsApi.GetApiProcess().GetRetrieveComplete().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<CmdSno> {sCmdSno} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunManualCommandCancel(string sCmdSno, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                        {
                            int iRet_Task = TaskTable.CheckHasTaskCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            int iRet_Teach = LocMst.GetTeachLoc_byBoxID(cmd.BoxID, db);
                            if (iRet_Teach == DBResult.Exception)
                            {
                                strEM = "取得校正儲位資料失敗！";
                                return false;
                            }

                            if (iRet_Task == DBResult.Success) TaskTable.FunInsertHisTask(cmd.CmdSno, db);
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Cancel, "WCS命令手動取消", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (iRet_Teach == DBResult.Success)
                            {
                                if (!LocMst.FunClearTeachLoc_byBoxID(cmd.BoxID, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            WcsCancelInfo info = new WcsCancelInfo
                            {
                                carrierId = cmd.BoxID,
                                jobId = cmd.JobID
                            };

                            if (cmd.CmdMode == clsConstValue.CmdMode.StockIn)
                            {
                                info.cancelType = clsEnum.WmsApi.CancelType.PUTAWAY.ToString();
                            }
                            else if (cmd.CmdMode == clsConstValue.CmdMode.L2L)
                            {
                                info.cancelType = clsEnum.WmsApi.CancelType.SHELF.ToString();
                            }
                            else
                            {
                                info.cancelType = clsEnum.WmsApi.CancelType.RETRIEVE.ToString();
                            }

                            if (!clsWmsApi.GetApiProcess().GetWcsCancel().FunReport(info))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<CmdSno> {sCmdSno} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        

        public bool FunCalculateUtilization(DateTime startTime, DateTime endTime, string eqpId, ref double upTimeInHrs, ref double downTimeInHrs, ref int CmdCount, ref int AlarmCount, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        DataTable dtTmp = new DataTable();
                        #region 取得Uptime的時間
                        int upTime = 0;
                        if (unitStsLog.FunGetUptimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {

                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                upTime = upTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        upTimeInHrs = Math.Round((Convert.ToDouble(upTime) / 3600.0), 2, MidpointRounding.AwayFromZero);
                        dtTmp = null;
                        #endregion 取得Uptime的時間
                        #region 取得Downtime的時間
                        int downTime = 0;
                        if (unitStsLog.FunGetDowntimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {
                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                downTime = downTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        downTimeInHrs = Math.Round((Convert.ToDouble(downTime) / 3600.0), 2, MidpointRounding.AwayFromZero);
                        dtTmp = null;
                        #endregion 取得Downtime的時間
                        #region 計算CmdCount和AlarmCount
                        CmdCount = 0;
                        AlarmCount = 0;
                        if (CMD_MST_HIS.FunGetCmdBetweenTime(startTime, endTime, eqpId, ref dtTmp, db))
                        {
                            CmdCount = dtTmp.Rows.Count;
                        }
                        else
                        {

                        }
                        dtTmp = null;
                        if (alarmData.FunGetAlarmBetweenTime(startTime, endTime, eqpId, ref dtTmp, db))
                        {
                            AlarmCount = dtTmp.Rows.Count;
                        }
                        else
                        {

                        }
                        dtTmp = null;
                        #endregion 計算CmdCount和AlarmCount
                        return true;
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunCalculateOEEUtilization(DateTime startTime, DateTime endTime, string eqpId, ref double upTimeInSec, ref double downTimeInSec, ref double runTimeInSec, ref double repairTimeInSec, ref double availableTimeInSec, ref double PlanTime, ref int PlanCount, ref int AlarmCount, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        DataTable dtTmp = new DataTable();
                        #region 取得Uptime的時間
                        int upTime = 0;
                        DateTime firstT, lastT;
                        if (unitStsLog.FunGetUptimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {

                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                upTime = upTime + tempTime;
                            }
                            firstT = Convert.ToDateTime(dtTmp.Rows[0]["STRDT"]);
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dtTmp.Rows[dtTmp.Rows.Count-1]["ENDDT"])))
                            {
                                lastT = Convert.ToDateTime(dtTmp.Rows[dtTmp.Rows.Count - 1]["ENDDT"]);
                            }
                            else
                            {
                                lastT = endTime;
                            }
                            availableTimeInSec = (lastT - firstT).TotalSeconds;
                        }
                        else
                        {
                            availableTimeInSec = 0;
                        }
                        upTimeInSec = upTime;
                        dtTmp = null;
                        #endregion 取得Uptime的時間
                        #region 取得Runtime的時間
                        int runTime = 0;
                        if (unitStsLog.FunGetRuntimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {

                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                runTime = runTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        runTimeInSec = runTime;
                        dtTmp = null;
                        #endregion 取得Runtime的時間
                        #region 取得Downtime的時間
                        int downTime = 0;
                        if (unitStsLog.FunGetDowntimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {
                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                downTime = downTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        downTimeInSec = downTime;
                        dtTmp = null;
                        #endregion 取得Downtime的時間
                        #region 取得Repairtime的時間
                        int repairTime = 0;
                        if (unitStsLog.FunGetRepairtimeStsLog(startTime, endTime, eqpId, ref dtTmp, db))
                        {

                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                repairTime = repairTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        repairTimeInSec = repairTime;
                        dtTmp = null;
                        #endregion 取得Repairtime的時間
                        #region 計算AlarmCount
                        AlarmCount = 0;
                        if (alarmData.FunGetAlarmBetweenTime(startTime, endTime, eqpId, ref dtTmp, db))
                        {
                            AlarmCount = dtTmp.Rows.Count;
                        }
                        else
                        {

                        }
                        dtTmp = null;
                        #endregion 計算AlarmCount
                        PlanTime = Convert.ToDouble(_config_OEEParam.PlanTime) * 3600;
                        PlanCount = _config_OEEParam.PlanCount[Convert.ToInt32(eqpId) - 1];
                        return true;
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunCalculatePortOEEUtilization(DateTime startTime, DateTime endTime, ref double upTimeInSec, ref double downTimeInSec, ref double runTimeInSec, ref double availableTimeInSec, ref double PlanTime, ref int PlanCount, ref int AlarmCount, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        DataTable dtTmp = new DataTable();
                        #region 取得Uptime的時間
                        int upTime = 0;
                        DateTime firstT, lastT;
                        if (unitStsLog.FunGetPortUptimeStsLog(startTime, endTime, ref dtTmp, db))
                        {

                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                upTime = upTime + tempTime;
                            }
                            firstT = Convert.ToDateTime(dtTmp.Rows[0]["STRDT"]);
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dtTmp.Rows[dtTmp.Rows.Count-1]["ENDDT"])))
                            {
                                lastT = Convert.ToDateTime(dtTmp.Rows[dtTmp.Rows.Count - 1]["ENDDT"]);
                            }
                            else
                            {
                                lastT = endTime;
                            }
                            availableTimeInSec = (lastT - firstT).TotalSeconds;
                        }
                        else
                        {
                            availableTimeInSec = 0;
                        }
                        upTimeInSec = upTime;
                        runTimeInSec = upTime;
                        dtTmp = null;
                        #endregion 取得Uptime的時間
                        #region 取得Downtime的時間
                        int downTime = 0;
                        if (unitStsLog.FunGetPortDowntimeStsLog(startTime, endTime, ref dtTmp, db))
                        {
                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                int tempTime = Convert.ToInt32(dtTmp.Rows[i]["TotalSecs"]);
                                downTime = downTime + tempTime;
                            }
                        }
                        else
                        {

                        }
                        downTimeInSec = downTime;
                        dtTmp = null;
                        #endregion 取得Downtime的時間
                        #region 計算AlarmCount
                        AlarmCount = 0;
                        for (int portID = 1; portID < 4; portID++)
                        {
                            if (alarmData.FunGetPortAlarm(startTime, endTime, Convert.ToString(portID), ref dtTmp, db))
                            {
                                AlarmCount = AlarmCount + dtTmp.Rows.Count;
                            }
                            else
                            {

                            }
                        }
                        dtTmp = null;
                        #endregion 計算AlarmCount
                        PlanTime = Convert.ToDouble(_config_OEEParam.PlanTime) * 3600;
                        PlanCount = _config_OEEParam.PlanCount.Sum();
                        return true;
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool GetNewCmdForS2S(string BoxID, string EquNo, ConveyorInfo buffer, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        cmd.CmdSno = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                        if (string.IsNullOrWhiteSpace(cmd.CmdSno))
                        {
                            strEM = "<" + BoxID + ">取得序號失敗！";
                            clsWriLog.Log.FunWriTraceLog_CV(strEM);
                            return false;
                        }

                        int[] checkPortAvailable = new int[3];
                        bool checkNormal = false;
                        for (int i = 0; i < 3; i++)
                        {
                            ConveyorInfo bufferCheck = i switch
                            {
                                0 => ConveyorDef.A1_41,
                                1 => ConveyorDef.A1_42,
                                _ => ConveyorDef.A1_43,
                            };
                            if (clsMicronCV.GetConveyorController().GetBuffer(bufferCheck.Index).Auto &&
                                !clsMicronCV.GetConveyorController().GetBuffer(bufferCheck.Index).Error)
                            {
                                checkPortAvailable[i] = (int)clsEnum.IOPortStatus.NORMAL;
                                checkNormal = true;
                                cmd.StnNo = bufferCheck.StnNo;
                                break;
                            }
                        }
                        if (!checkNormal)
                        {
                            cmd.StnNo = ConveyorDef.A1_41.StnNo;
                        }
                        cmd.backupPortId = "";
                        cmd.BatchID = "";
                        cmd.BoxID = BoxID;
                        cmd.CmdMode = clsConstValue.CmdMode.S2S;
                        cmd.CurDeviceID = "";
                        cmd.CurLoc = "";
                        cmd.EndDate = "";
                        cmd.Loc = "";
                        cmd.EquNo = EquNo;
                        cmd.ExpDate = "";
                        cmd.IoType = clsConstValue.CmdMode.S2S;
                        cmd.JobID = "ToNGPort";
                        cmd.NeedShelfToShelf = clsEnum.NeedL2L.N.ToString();
                        cmd.NewLoc = "";
                        cmd.Prt = "5";
                        cmd.Userid = "WCS";
                        cmd.ZoneID = "";

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            strEM = "Error: Begin失敗！";
                            clsWriLog.Log.FunWriTraceLog_CV(strEM);
                            return false;
                        }

                        if (!CMD_MST.FunInsCmdMst(cmd, ref strEM, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        ConveyorInfo NextBuffer = clsMicronCV.GetNextBuffer(buffer);

                        if (clsMicronCV.GetConveyorController().GetBuffer(NextBuffer.Index).Presence)
                        {
                            if (clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAndSetReadReqAsync(cmd.CmdSno, int.Parse(cmd.CmdMode), buffer.Path).Result)
                                clsWriLog.Log.FunWriTraceLog_CV($"<Buffer> {buffer.BufferName} <任務號> {cmd.CmdSno} => 路徑寫入成功！ ({buffer.Path})");
                            else
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                clsWriLog.Log.FunWriTraceLog_CV($"NG: <Buffer> {buffer.BufferName} <路徑> {buffer.Path} => 填寫路徑失敗！");
                                return false;
                            }
                        }
                        else
                        {
                            if (clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).WriteCommandAndSetReadReqAsync(cmd.CmdSno, int.Parse(cmd.CmdMode), NextBuffer.Path).Result)
                                clsWriLog.Log.FunWriTraceLog_CV($"<Buffer> {buffer.BufferName} <任務號> {cmd.CmdSno} => 路徑寫入成功！ ({NextBuffer.Path})");
                            else
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                clsWriLog.Log.FunWriTraceLog_CV($"NG: <Buffer> {buffer.BufferName} <路徑> {NextBuffer.Path} => 填寫路徑失敗！");
                                return false;
                            }
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        if (BoxID != "ID_ERR")
                        {
                            WcsCancelInfo info = new WcsCancelInfo
                            {
                                carrierId = cmd.BoxID,
                                jobId = cmd.JobID
                            };

                            info.cancelType = clsEnum.WmsApi.CancelType.PUTAWAY.ToString();
                            clsWmsApi.GetApiProcess().GetWcsCancel().FunReport(info);

                            info.cancelType = clsEnum.WmsApi.CancelType.SHELF.ToString();
                            clsWmsApi.GetApiProcess().GetWcsCancel().FunReport(info);

                            info.cancelType = clsEnum.WmsApi.CancelType.RETRIEVE.ToString();
                            clsWmsApi.GetApiProcess().GetWcsCancel().FunReport(info);
                        }

                        return true;
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunManualCreateToTeachCmd_Proc(string sBoxID, ref string sRemark)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db, ref sRemark);
                    if (iRet == DBResult.Success)
                    {
                        int iStockerID = 0; string sLoc = "";
                        iRet = proc.GetWMS_DBObject().GetLocMst().CheckLocByBoxID(sBoxID, ref iStockerID, ref sLoc);
                        if (iRet != DBResult.Success)
                        {
                            sRemark = $"<BoxID>{sBoxID} => 取得WMS儲位資訊失敗！";
                            return false;
                        }

                        return proc.SubCreateToTeachCmd_Proc(iStockerID, sBoxID, sLoc, ref sRemark, db);
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public void FunCreateToTeachCmd_Proc(DataTable dtTmp)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        for (int i = 0; i < dtTmp.Rows.Count; i++)
                        {
                            string sBoxID = Convert.ToString(dtTmp.Rows[i]["BoxID"]);
                            int iStockerID = 0; string sLoc = "";
                            iRet = proc.GetWMS_DBObject().GetLocMst().CheckLocByBoxID(sBoxID, ref iStockerID, ref sLoc);
                            if (iRet != DBResult.Success) continue;

                            iRet = CMD_MST.FunCheckHasCommand(iStockerID, db);
                            if (iRet == DBResult.Success || iRet == DBResult.Exception) continue;

                            string sRemark = "";
                            if (proc.SubCreateToTeachCmd_Proc(iStockerID, sBoxID, sLoc, ref sRemark, db)) return;
                            else continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            }
        }

        public bool FunInsCmdForManual(CmdMstInfo cmd, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            strEM = "Begin失敗！";
                            clsWriLog.Log.FunWriTraceLog_CV(strEM);
                            return false;
                        }

                        if (!CMD_MST.FunInsCmdMst(cmd, ref strEM, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        ConveyorInfo buffer = clsMicronCV.GetBufferByStnNo(cmd.StnNo);
                        if (!clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).SetManualPutaway().Result)
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            clsWriLog.Log.FunWriTraceLog_CV($"NG: <Buffer> {buffer.BufferName} 手動模式失敗！");
                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        return true;
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        */
        #endregion
    }
}
