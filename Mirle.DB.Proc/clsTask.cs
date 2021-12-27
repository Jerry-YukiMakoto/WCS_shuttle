using System;
using Mirle.DataBase;
using System.Linq;
using System.Data;
using Mirle.Def;
using Mirle.Structure;

namespace Mirle.DB.Proc
{
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
}
