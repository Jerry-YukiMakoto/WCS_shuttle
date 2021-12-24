using System;
using Mirle.DataBase;
using System.Linq;
using System.Data;
using Mirle.Def;
using Mirle.Structure;

namespace Mirle.DB.Proc
{
    #region Mark
    /*
public class clsTask
{
    private Fun.clsTask task = new Fun.clsTask();
    private Fun.clsCmd_Mst cmd_Mst = new Fun.clsCmd_Mst();
    private Fun.clsLocMst locMst = new Fun.clsLocMst();
    private Fun.clsProc proc;
    private Fun.clsSno SNO = new Fun.clsSno();
    private clsDbConfig _config = new clsDbConfig();
    private clsDbConfig _config_Sqlite = new clsDbConfig();
    public clsTask(clsDbConfig config, clsDbConfig config_WMS, clsDbConfig config_Sqlite)
    {
        _config = config;
        _config_Sqlite = config_Sqlite;
        proc = new Fun.clsProc(config_WMS);
    }

    public int GetForkPickupCmd_ForStockOut(ref DataTable dtTmp)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    return task.GetForkPickupCmd_ForStockOut(ref dtTmp, db);
                }

                return iRet;
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            return DBResult.Exception;
        }
    }

    public int GetForkCommand(int StockerID, int Fork, ref CmdMstInfo cmd)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    return task.GetForkCommand(StockerID, Fork, ref cmd, db);
                }

                return iRet;
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            return DBResult.Exception;
        }
    }

    public int CheckHasTaskCmd(int StockerID)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    return task.CheckHasTaskCmd(StockerID, db);
                }

                return iRet;
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            return DBResult.Exception;
        }
    }

    public int CheckHasTaskCmd(string CommandID)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    return task.CheckHasTaskCmd(CommandID, db);
                }

                return iRet;
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            return DBResult.Exception;
        }
    }

    public bool FunDelTaskCmd_Proc(string sCmdSno)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    iRet = task.FunSelectTaskCmdByCommandID(sCmdSno, db);
                    if(iRet == DBResult.Success)
                    {
                        task.FunInsertHisTask(sCmdSno, db);
                        return task.FunDelTaskCmd(sCmdSno, db);
                    }
                    else
                    {
                        if (iRet == DBResult.NoDataSelect) return true;
                        else return false;
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

    public void FunCheckTaskCmdFinish()
    {
        DataTable dtTmp = new DataTable();
        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    iRet = task.GetFinishCommand(ref dtTmp, db);
                    if (iRet == DBResult.Success)
                    {
                        for (int i = 0; i < dtTmp.Rows.Count; i++)
                        {
                            string sDeviceID = Convert.ToString(dtTmp.Rows[i]["DeviceID"]);
                            string sCmdSno = Convert.ToString(dtTmp.Rows[i]["CommandID"]);
                            string sTaskNo = Convert.ToString(dtTmp.Rows[i]["TaskNo"]);
                            string strCompleteCode = Convert.ToString(dtTmp.Rows[i]["CompleteCode"]);
                            string sBoxID = Convert.ToString(dtTmp.Rows[i]["CSTID"]);
                            string sTaskMode = Convert.ToString(dtTmp.Rows[i]["TransferMode"]);
                            string sFinishLoc = Convert.ToString(dtTmp.Rows[i]["FinishLocation"]);
                            int fork = Convert.ToInt32(dtTmp.Rows[i]["ForkNo"]);
                            int iMode = int.Parse(sTaskMode);
                            string sSource = Convert.ToString(dtTmp.Rows[i]["Source"]);
                            int.TryParse(sSource, out int iSource);
                            string sDestination = Convert.ToString(dtTmp.Rows[i]["Destination"]);
                            int.TryParse(sDestination, out int iDest);

                            if (iMode == (int)clsEnum.TaskMode.Move)
                            {
                                task.FunInsertHisTask(sCmdSno, db);
                                task.FunDelTaskCmd(sCmdSno, db);
                                continue;
                            }

                            clsEnum.LocType locType_Dest;
                            clsEnum.LocType locType_Source;
                            if (iSource > 100) locType_Source = clsEnum.LocType.Shelf;
                            else locType_Source = clsEnum.LocType.Port;

                            if (iDest > 100) locType_Dest = clsEnum.LocType.Shelf;
                            else locType_Dest = clsEnum.LocType.Port;

                            CmdMstInfo cmd = new CmdMstInfo();
                            if (cmd_Mst.FunGetCommand(sCmdSno, ref cmd, ref iRet, db) == false)
                            {
                                if(iRet == DBResult.NoDataSelect)
                                {
                                    if(task.FunInsertHisTask(sCmdSno, db))
                                    {
                                        task.FunDelTaskCmd(sCmdSno, db);
                                    }
                                }

                                continue;
                            }

                            if (iMode == (int)clsEnum.TaskMode.Pickup)
                            {
                                switch (strCompleteCode)
                                {
                                    case clsConstValue.CompleteCode.Success_FromReturnCodeAck:
                                    case clsConstValue.CompleteCode.Success_ToReturnCode:
                                        //if (proc.FunPickupFinish_Proc(cmd, sDeviceID, fork, db)) return;
                                        //else continue;
                                        proc.FunPickupFinish_Proc(cmd, sDeviceID, fork, db); break;
                                    case clsConstValue.CompleteCode.EmptyRetrieval:
                                        //if (proc.FunEmptyRetrieval_Proc(cmd, sDeviceID, db)) return;
                                        //else continue;
                                        proc.FunEmptyRetrieval_Proc(cmd, sDeviceID, locType_Source, db); break;
                                    default:
                                        //if (proc.FunAbnormalFinish_Proc(cmd, clsEnum.TaskMode.Pickup,
                                        //    sDeviceID, sFinishLoc, sSource, sDestination, db))
                                        //    return;
                                        //else continue;
                                        proc.FunAbnormalFinish_Proc(cmd, clsEnum.TaskMode.Pickup,
                                            sDeviceID, sFinishLoc, sSource, sDestination, db);
                                        break;
                                }
                            }
                            else if (iMode == (int)clsEnum.TaskMode.Deposit)
                            {
                                switch (strCompleteCode)
                                {
                                    case clsConstValue.CompleteCode.Success_FromReturnCodeAck:
                                    case clsConstValue.CompleteCode.Success_ToReturnCode:
                                        if (locType_Dest == clsEnum.LocType.Port)
                                        {
                                            #region 置物至CV
                                            //if (proc.DepositToPortFinish_Proc(cmd, sDeviceID, iDest, db)) return;
                                            //else continue;
                                            proc.DepositToPortFinish_Proc(cmd, sDeviceID, iDest, db); break;
                                            #endregion 置物至CV
                                        }
                                        else
                                        {
                                            #region 置物至儲位
                                            //if (proc.DepositToShelfOrTeachFinish_Proc(cmd, sDeviceID, sDestination, db)) return;
                                            //else continue;
                                            proc.DepositToShelfOrTeachFinish_Proc(cmd, sDeviceID, sDestination, db); break;
                                            #endregion 置物至儲位
                                        }
                                    case clsConstValue.CompleteCode.DoubleStorage:
                                        //if (proc.FunDoubleStorage_Proc(cmd, sDeviceID, fork, locType_Dest, iDest, db)) return;
                                        //else continue;
                                        proc.FunDoubleStorage_Proc(cmd, sDeviceID, fork, locType_Dest, iDest, db); break;
                                    default:
                                        //if (proc.FunAbnormalFinish_Proc(cmd, clsEnum.TaskMode.Deposit,
                                        //    sDeviceID, sFinishLoc, sSource, sDestination, db))
                                        //    return;
                                        //else continue;
                                        proc.FunAbnormalFinish_Proc(cmd, clsEnum.TaskMode.Deposit,
                                            sDeviceID, sFinishLoc, sSource, sDestination, db);
                                        break;
                                }
                            }
                            else continue;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
        }
        finally
        {
            dtTmp = null;
        }
    }

    public void FunCheckAndDeleteTaskCmd(string sCmdSno, int StockerID)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (task.CheckHasTaskCmd(sCmdSno, db) == DBResult.Success)
                    {
                        task.FunInsertHisTask(sCmdSno, db);

                        Location loc = MicronLocation.GetLocation_ByStockOutPort(StockerID);
                        if (loc == null)
                        {
                            clsWriLog.Log.FunWriTraceLog_CV($"Error: <CmdSno> {sCmdSno} => 取得目的站Location失敗！");
                            return;
                        }
                        else
                        {
                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success) return;

                            if (!cmd_Mst.FunUpdateCurLoc(sCmdSno, loc.DeviceId, loc.LocationId, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return;
                            }

                            if(!task.FunDelTaskCmd(sCmdSno, db))
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return;
                            }

                            db.TransactionCtrl(TransactionTypes.Commit);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
        }
    }

    public bool FunDelHisTask(double dblDay)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    return task.FunDelHisTask(dblDay, db);
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

    public void FunReUpdateTaskCmd_Proc(int StockerID)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    var stkTaskService = clsMicronStocker.GetSTKCHostById(StockerID).GetTaskCommandService();
                    if (stkTaskService.unUpdateTaskCmdList.Count() > 0)
                    {
                        foreach(var lst in stkTaskService.unUpdateTaskCmdList)
                        {
                            iRet = task.FunSelectTaskCmd(lst.TaskNo, db);
                            if (iRet == DBResult.Success)
                            {
                                if (task.UpdateByTaskNo_ReturnInt(lst, db) == DBResult.Success)
                                {
                                    stkTaskService.unUpdateTaskCmdList.Remove(lst);
                                    return;
                                }
                            }
                            else if(iRet == DBResult.NoDataSelect)
                            {
                                stkTaskService.unUpdateTaskCmdList.Remove(lst);
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
        }
    }

    public bool FunInsertTaskCmd(string CommandID, string DeviceID, clsEnum.TaskMode mode, string sFrom, string sTo, int ForkNo)
    {
        try
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (task.funCheckTaskCmdRepeat(DeviceID, ForkNo, db)) return false;
                    string TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                    if (string.IsNullOrWhiteSpace(TaskNo)) return false;
                    string strEM = "";
                    return task.FunInsertTaskCmd(CommandID, TaskNo, DeviceID, mode, sFrom, sTo, ref strEM, 1, "move", 100, ForkNo, db);
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
    
    }
    */
    #endregion
}
