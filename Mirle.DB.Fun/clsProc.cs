using System;
using Mirle.Def;
using Mirle.Def.U2NMMA30;
using Mirle.DataBase;
using Mirle.Micron.U2NMMA30;
using Mirle.Structure.Info;
using Mirle.Structure;
using System.Windows.Forms;
using Mirle.Grid.U2NMMA30;
using Mirle.WebAPI.U2NMMA30.ReportInfo;

namespace Mirle.DB.Fun
{
    public class clsProc
    {
        private clsCmd_Mst CMD_MST = new clsCmd_Mst();
        private clsTask TaskTable = new clsTask();
        private clsSno SNO = new clsSno();
        private clsLocMst locMst = new clsLocMst();
        private WMS.Proc.clsHost wms;
        private clsL2LCount L2LCount = new clsL2LCount();
        public clsProc(clsDbConfig dbConfig_WMS)
        {
            wms = new WMS.Proc.clsHost(dbConfig_WMS);
        }

        public WMS.Proc.clsHost GetWMS_DBObject()
        {
            return wms;
        }


        public int CheckLocDDHasNeedL2LCmd(CmdMstInfo cmd, string sDeviceID, string sCurLoc, ref string sCmdSno_DD, SqlServer db)
        {
            try
            {
                clsEnum.Fork fork = clsEnum.Fork.None;
                if (sDeviceID == "4") return DBResult.NoDataSelect;            //Single Deep不用判斷
                else if (
                          Micron.U2NMMA30.clsTool.IsLimit(cmd.Loc, ref fork) &&
                          (sCurLoc == LocationDef.Location.LeftFork.ToString() || sCurLoc == LocationDef.Location.RightFork.ToString())
                        )
                {   //目前位置在Fork上且來源儲位是極限位置時先不判斷
                    return DBResult.NoDataSelect;
                }
                else
                {
                    bool IsOutside = false; string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                    int iRet =  wms.GetLocMst().CheckLocIsOutside(cmd.Loc, ref IsOutside, ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                    if (iRet == DBResult.Success)
                    {
                        if (IsOutside || IsEmpty_DD) return DBResult.NoDataSelect;
                        else
                        {
                            return CMD_MST.CheckHasNeedL2LCmd(sLocDD, ref sCmdSno_DD, db);
                        }
                    }
                    else throw new Exception("Error: 確認儲位是否是外儲位失敗！");
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }

       

        

        public bool FunRepeatPickFromShelf(CmdMstInfo cmd, string DeviceID, string sFrom, SqlServer db)
        {
            try
            {
                bool IsTeach = false;
                int iRet = locMst.CheckIsTeach(DeviceID, sFrom, ref IsTeach, db);
                if (iRet == DBResult.Exception) return false;
                else
                {
                    TaskTable.FunInsertHisTask(cmd.CmdSno, db);
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

                    if(IsTeach)
                    {
                        if (!CMD_MST.FunUpdateCurLoc(cmd.CmdSno, DeviceID, LocationDef.Location.Teach.ToString(), db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        if (!locMst.FunUpdLocSts(DeviceID, sFrom, clsEnum.LocSts.S, cmd.BoxID, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }
                    }
                    else
                    {
                        if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Initial, "", db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }
                    }

                    if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDepositToTeachFail_Proc(CmdMstInfo cmd, string DeviceID, string sLoc, int fork, SqlServer db)
        {
            try
            {
                string sCurLoc = fork == (int)clsEnum.Fork.Left ? LocationDef.Location.LeftFork.ToString() : LocationDef.Location.RightFork.ToString();

                PositionReportInfo info = new PositionReportInfo
                {
                    carrierId = cmd.BoxID,
                    jobId = cmd.JobID,
                    location = sCurLoc
                };

                TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                if (!CMD_MST.FunUpdateCurLoc(cmd.CmdSno, DeviceID, sCurLoc, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!locMst.FunUpdLocSts(DeviceID, sLoc, clsEnum.LocSts.N, "", db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!clsWmsApi.GetApiProcess().GetPositionReport().FunReport(info))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                db.TransactionCtrl(TransactionTypes.Commit);
                return true;
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

       

        private bool FunDepositToPortFinish_Proc(CmdMstInfo cmd, Location loc, SqlServer db)
        {
            try
            {
                if (cmd.CmdMode != clsConstValue.CmdMode.S2S)
                {
                    string sCmdSno_DD = ""; int iRet = DBResult.Initial;
                    bool IsTeach = false;
                    iRet = locMst.CheckIsTeach(loc.DeviceId, Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(cmd.Loc), ref IsTeach, db);
                    if (iRet == DBResult.Exception) return false;

                    if (!IsTeach)
                    {
                        iRet = CheckLocDDHasNeedL2LCmd(cmd, loc.DeviceId, loc.LocationId, ref sCmdSno_DD, db);
                        if (iRet == DBResult.Exception) return false;
                    }
                    else iRet = DBResult.NoDataSelect;

                    PositionReportInfo info = new PositionReportInfo
                    {
                        carrierId = cmd.BoxID,
                        jobId = cmd.JobID,
                        location = loc.LocationId
                    };

                    TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                    if (!CMD_MST.FunUpdateCurLoc(cmd.CmdSno, loc.DeviceId, loc.LocationId, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (iRet == DBResult.Success)
                    {
                        if (!CMD_MST.FunUpdateNeedL2L(sCmdSno_DD, clsEnum.NeedL2L.N, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }
                    }

                    if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (!clsWmsApi.GetApiProcess().GetPositionReport().FunReport(info))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);

                    L2LCount.FunDelL2LCount(cmd.BoxID, ref sRemark, db);

                    return true;
                }
                else
                {
                    TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                    if (!CMD_MST.FunUpdateCurLocForS2S(cmd.CmdSno, loc.DeviceId, loc.LocationId, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);

                    L2LCount.FunDelL2LCount(cmd.BoxID, ref sRemark, db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool DepositToShelfOrTeachFinish_Proc(CmdMstInfo cmd, string DeviceID, string sTo, SqlServer db)
        {
            try
            {
                bool IsTeach = false;
                int iRet = locMst.CheckIsTeach(DeviceID, sTo, ref IsTeach, db);
                if (iRet == DBResult.Exception) return false;
                else
                {
                    if (IsTeach) return FunDepositToTeachFinish_Proc(cmd, DeviceID, sTo, db);
                    else return FunDepositToShelfFinish_Proc(cmd, db);
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        private bool FunDepositToTeachFinish_Proc(CmdMstInfo cmd, string DeviceID, string Loc, SqlServer db)
        {
            try
            {
                string sCmdSno_DD = "";
                int iRet = CheckLocDDHasNeedL2LCmd(cmd, DeviceID, LocationDef.Location.Teach.ToString(), ref sCmdSno_DD, db);
                if (iRet == DBResult.Exception) return false;

                PositionReportInfo info = new PositionReportInfo
                {
                    carrierId = cmd.BoxID,
                    jobId = cmd.JobID,
                    location = Loc
                };

                TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                if (!locMst.FunUpdLocSts(DeviceID, Loc, clsEnum.LocSts.S, cmd.BoxID, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!CMD_MST.FunUpdateCurLoc(cmd.CmdSno, DeviceID, LocationDef.Location.Teach.ToString(), db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (iRet == DBResult.Success)
                {
                    if (!CMD_MST.FunUpdateNeedL2L(sCmdSno_DD, clsEnum.NeedL2L.N, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }
                }

                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!clsWmsApi.GetApiProcess().GetPositionReport().FunReport(info))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                db.TransactionCtrl(TransactionTypes.Commit);
                return true;
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        private bool FunDepositToShelfFinish_Proc(CmdMstInfo cmd, SqlServer db)
        {
            try
            {
                string sFinishLoc = "";
                if (cmd.CmdMode == clsConstValue.CmdMode.L2L) sFinishLoc = cmd.NewLoc;
                else sFinishLoc = cmd.Loc;

                PositionReportInfo info = new PositionReportInfo
                {
                    carrierId = cmd.BoxID,
                    jobId = cmd.JobID,
                    location = sFinishLoc
                };

                TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                if (!CMD_MST.FunUpdateCurLoc(cmd.CmdSno, Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sFinishLoc).ToString(), 
                    LocationDef.Location.Shelf.ToString(), db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!clsWmsApi.GetApiProcess().GetPositionReport().FunReport(info))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                db.TransactionCtrl(TransactionTypes.Commit);
                return true;
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunEmptyRetrieval_Proc(CmdMstInfo cmd, string DeviceID, clsEnum.LocType locType, SqlServer db)
        {
            try
            {
                string sTeachLoc = ""; string sLoc_Empty = "";
                if (cmd.CurLoc == LocationDef.Location.Teach.ToString())
                {
                    sTeachLoc = locMst.GetLoc(cmd.BoxID, db);
                    if (!string.IsNullOrWhiteSpace(sTeachLoc))
                    {
                        sLoc_Empty = Micron.U2NMMA30.clsTool.FunTaskLocToWmsLoc(sTeachLoc, DeviceID);
                    }
                }
                else sLoc_Empty = cmd.Loc;

                TaskTable.FunInsertHisTask(cmd.CmdSno, db);

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

                if (locType == clsEnum.LocType.Port)
                {
                    sRemark = $"Error: 從站口取物失敗！";
                    if (!CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);
                    return true;
                }
                else
                {
                    if (cmd.CurLoc == LocationDef.Location.Teach.ToString())
                    {
                        if (string.IsNullOrWhiteSpace(sTeachLoc))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);

                            sRemark = "Error: 找不到校正儲位！";
                            if (sRemark != cmd.Remark)
                            {
                                CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                            }

                            return false;
                        }
                        else
                        {
                            if (!locMst.FunUpdLocSts(DeviceID, sTeachLoc, clsEnum.LocSts.N, "", db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }
                        }
                    }

                    sRemark = "Error: 空出庫，命令異常結束！";
                    if (!CMD_MST.FunUpdateCmdSts(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Finished, clsEnum.Cmd_Abnormal.E2, sRemark, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (cmd.CmdMode == clsConstValue.CmdMode.StockOut)
                    {
                        RetrieveCompleteInfo info = new RetrieveCompleteInfo
                        {
                            carrierId = cmd.BoxID,
                            isComplete = clsEnum.WmsApi.IsComplete.Y.ToString(),
                            jobId = cmd.JobID,
                            portId = cmd.StnNo,
                            emptyTransfer = clsEnum.WmsApi.IsComplete.Y.ToString()
                        };

                        if (!clsWmsApi.GetApiProcess().GetRetrieveComplete().FunReport(info))
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
                            emptyTransfer = clsEnum.WmsApi.IsComplete.Y.ToString(),
                            shelfId = sLoc_Empty
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
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDoubleStorage_Proc(CmdMstInfo cmd, string DeviceID, int fork, clsEnum.LocType locType_Dest, int iTo, SqlServer db)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            try
            {
                string sRemark = "";
                if(locType_Dest == clsEnum.LocType.Port)
                {
                    #region 站口二重格
                    string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                    if (string.IsNullOrWhiteSpace(sTaskNo))
                    {
                        sRemark = "Error: 取得TaskNo失敗！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                        }

                        return false;
                    }

                    TaskTable.FunInsertHisTask(cmd.CmdSno, db);

                    if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                    {
                        sRemark = "Begin失敗";
                        clsWriLog.Log.FunWriTraceLog_CV(cmet.DeclaringType.FullName + "." + cmet.Name + " => " + sRemark);
                        return false;
                    }

                    if (TaskTable.FunDelTaskCmd(cmd.CmdSno, db) == false)
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (TaskTable.FunInsertTaskCmd(cmd.CmdSno, sTaskNo, DeviceID, clsEnum.TaskMode.Deposit, clsMicronStocker.GetShelfIdByFork(fork), 
                        iTo.ToString(), ref sRemark, 1, cmd.BoxID, clsMicronStocker.GetStockerById(int.Parse(DeviceID)).GetCraneById(1).Speed, fork, db) == false)
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);
                    return true;
                    #endregion 站口二重格
                }
                else
                {
                    EmptyShelfQuery_WCS emptyShelfQueryInfo = new EmptyShelfQuery_WCS
                    {
                        jobId = cmd.JobID,
                        carrierId = cmd.BoxID
                    };
                    EmptyShelfQuery_WMS emptyShelfQueryResponse = new EmptyShelfQuery_WMS();
                    if(!clsWmsApi.GetApiProcess().GetEmptyShelfQuery().funReport(emptyShelfQueryInfo, ref emptyShelfQueryResponse))
                    {
                        sRemark = "Error: 發生二重格，取得新儲位失敗！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                        }
                        return false;
                    }
                    string sNewLoc = emptyShelfQueryResponse.shelfId;
                    if (string.IsNullOrWhiteSpace(sNewLoc))
                    {
                        sRemark = "Error: 發生二重格，但找不到新儲位可放！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                        }

                        return false;
                    }
                    else
                    {
                        int EquNo_New = Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc);
                        ShelfReportInfo info = new ShelfReportInfo
                        {
                            jobId = cmd.JobID,
                            shelfId = sNewLoc,
                            shelfStatus = clsConstValue.LocSts.IN,
                            carrierId = cmd.BoxID
                        };

                        TaskTable.FunInsertHisTask(cmd.CmdSno, db);

                        if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                        {
                            sRemark = "Error: Begin失敗！";
                            if (sRemark != cmd.Remark)
                            {
                                CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                            }

                            return false;
                        }

                        if (cmd.CmdMode == clsConstValue.CmdMode.L2L)
                        {
                            if (!CMD_MST.FunUpdateNewLocForL2L(cmd.CmdSno, sNewLoc, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }
                        }
                        else
                        {
                            if (!CMD_MST.FunUpdateLoc(cmd.CmdSno, sNewLoc, EquNo_New.ToString(), db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }
                        }

                        if (!TaskTable.FunDelTaskCmd(cmd.CmdSno, db))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        if (!clsWmsApi.GetApiProcess().GetShelfReport().FunReport(info))
                        {
                            db.TransactionCtrl(TransactionTypes.Rollback);
                            return false;
                        }

                        db.TransactionCtrl(TransactionTypes.Commit);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                db.TransactionCtrl(TransactionTypes.Rollback);
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunCreateTeachToFork_Proc(DataGridViewRow row, string TeachLoc, int fork, SqlServer db)
        {
            try
            {
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                string sBoxID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BoxId.Index].Value);
                string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                int iRet = TaskTable.CheckHasTaskCmd(sCmdSno, db);
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

                int Pry = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.PRT.Index].Value);

                if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                {
                    sRemark = "Error: Begin失敗！";
                    if (sRemark != sRemark_Pre)
                    {
                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                    }

                    return false;
                }

                if (!locMst.FunUpdLocSts(sCurDeviceID, TeachLoc, clsEnum.LocSts.O, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!TaskTable.FunInsertTaskCmd(sCmdSno, sTaskNo, sCurDeviceID, clsEnum.TaskMode.Pickup, TeachLoc,
                   clsMicronStocker.GetShelfIdByFork(fork), ref sRemark, Pry, sBoxID, clsMicronStocker.GetStockerById(int.Parse(sCurDeviceID)).GetCraneById(1).Speed, fork, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    if (sRemark != sRemark_Pre)
                    {
                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                    }

                    return false;
                }

                db.TransactionCtrl(TransactionTypes.Commit);
                return true;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunCreateForkToTeach_Proc(TransferBatch cmd, int StockerID, int fork, SqlServer db)
        {
            try
            {
                string sTeachLoc = locMst.GetLoc_ForDeposit(StockerID, db);
                string sRemark = "";
                if(string.IsNullOrWhiteSpace(sTeachLoc))
                {
                    sRemark = $"Error: Stocker{StockerID}的校正儲位無法預約！";
                    if (sRemark != cmd.Remark)
                    {
                        CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                    }

                    return false;
                }
                else
                {
                    int iRet = TaskTable.CheckHasTaskCmd(cmd.CommandID, db);
                    if (iRet == DBResult.Success)
                    {
                        sRemark = "Error: 該命令有設備命令正在執行！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                        }

                        return false;
                    }
                    else
                    {
                        if (iRet != DBResult.NoDataSelect)
                        {
                            sRemark = "Error: 檢查Task命令失敗！";
                            if (sRemark != cmd.Remark)
                            {
                                CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                            }

                            return false;
                        }
                    }

                    if (TaskTable.funCheckTaskCmdRepeat(StockerID.ToString(), fork, db))
                    {
                        sRemark = $"Error: Stocker{StockerID}的Fork{fork}還有命令正在執行！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                        }

                        return false;
                    }

                    string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                    if (string.IsNullOrWhiteSpace(sTaskNo))
                    {
                        sRemark = "Error: 取得TaskNo失敗！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                        }

                        return false;
                    }

                    int Pry = cmd.Priority;

                    if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                    {
                        sRemark = "Error: Begin失敗！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                        }

                        return false;
                    }

                    if (!locMst.FunUpdLocSts(StockerID.ToString(), sTeachLoc, clsEnum.LocSts.I, cmd.CarrierID, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }

                    if (!TaskTable.FunInsertTaskCmd(cmd.CommandID, sTaskNo, StockerID.ToString(), clsEnum.TaskMode.Deposit, clsMicronStocker.GetShelfIdByFork(fork),
                       sTeachLoc, ref sRemark, Pry, cmd.CarrierID, clsMicronStocker.GetStockerById(StockerID).GetCraneById(1).Speed, fork, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CommandID, sRemark, db);
                        }

                        return false;
                    }

                    db.TransactionCtrl(TransactionTypes.Commit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool SubCreateToTeachCmd_Proc(int iStockerID, string sBoxID, string sLoc, ref string sRemark, SqlServer db)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            try
            {
                CmdMstInfo cmd = new CmdMstInfo();
                int iRet = CMD_MST.FunGetCommand_byBoxID(sBoxID, ref cmd, db);
                if(iRet != DBResult.NoDataSelect)
                {
                    sRemark = $"<BoxID>{sBoxID} => 該料盒已有命令";
                    return false;
                }

                string sLoc_Teach = ""; string sDeviceID = "";
                if (iStockerID == 4) //Single Deep
                {
                    sLoc_Teach = locMst.GetLoc_ForDeposit(ref sDeviceID, db);
                }
                else
                {
                    sDeviceID = iStockerID.ToString();
                    sLoc_Teach = locMst.GetLoc_ForDeposit(iStockerID, db);
                }

                if (string.IsNullOrWhiteSpace(sLoc_Teach))
                {
                    sRemark = "找不到校正儲位！";
                    return false;
                }

                string sLoc_Teach_Sys = Micron.U2NMMA30.clsTool.FunTaskLocToWmsLoc(sLoc_Teach, sDeviceID);
                cmd = new CmdMstInfo();
                cmd.CmdSno = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                if (string.IsNullOrWhiteSpace(cmd.CmdSno))
                {
                    throw new Exception($"取得序號失敗！");
                }

                cmd.backupPortId = "";
                cmd.BatchID = "";
                cmd.BoxID = sBoxID;
                cmd.CmdMode = clsConstValue.CmdMode.L2L;
                cmd.CurDeviceID = "";
                cmd.CurLoc = "";
                cmd.EndDate = "";
                cmd.Loc = sLoc;
                cmd.EquNo = iStockerID.ToString();
                cmd.ExpDate = "";
                cmd.IoType = clsConstValue.CmdMode.L2L;
                cmd.JobID = DateTime.Now.ToString("yyyyMMddHHmmss");
                cmd.NeedShelfToShelf = clsEnum.NeedL2L.N.ToString();
                cmd.NewLoc = sLoc_Teach_Sys;
                cmd.Prt = "1";
                cmd.Remark = "";
                cmd.StnNo = "";
                cmd.Userid = "WCS";
                cmd.ZoneID = "";

                L2LCount.FunDelL2LCount(sBoxID, ref sRemark, db);
                if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                {
                    sRemark = $"{cmet.DeclaringType.FullName}.{cmet.Name} => Begin失敗！";
                    clsWriLog.Log.FunWriTraceLog_CV(sRemark);
                    return false;
                }

                if (!CMD_MST.FunInsCmdMst(cmd, ref sRemark, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                if (!locMst.FunUpdLocSts(sDeviceID, sLoc_Teach, clsEnum.LocSts.I, sBoxID, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                db.TransactionCtrl(TransactionTypes.Commit);
                return true;
            }
            catch (Exception ex)
            {
                sRemark = ex.Message;
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
    }
}
