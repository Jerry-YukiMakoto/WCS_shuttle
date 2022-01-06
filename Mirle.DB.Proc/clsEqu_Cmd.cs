using System;
using System.Data;
using Mirle.Def;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;

namespace Mirle.DB.Proc
{
    public class clsEqu_Cmd
    {
        private Fun.clsEqu_Cmd EQU_CMD = new Fun.clsEqu_Cmd();
        private Fun.clsCmd_Mst CMD_MST = new Fun.clsCmd_Mst();
        private clsSno sno;
        private Fun.clsProc proc;
        private clsDbConfig _config = new clsDbConfig();
        private clsDbConfig _config_Sqlite = new clsDbConfig();

        public clsEqu_Cmd(clsDbConfig config, clsDbConfig config_WMS, clsDbConfig config_Sqlite)
        {
            _config = config;
            _config_Sqlite = config_Sqlite;
            proc = new Fun.clsProc(config_WMS);
        }

        public GetDataResult checkCraneNoReapeat(out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.checkCraneNoReapeat(out dataObject, db);
                    }
                    else
                    {
                        dataObject = new DataObject<CmdMst>();
                        return GetDataResult.Initial;
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject = new DataObject<CmdMst>();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return GetDataResult.Exception;
            }
        }

        public void FunCheckEquCmdFinish()
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
                        iRet = EQU_CMD.GetFinishCommand(ref dtTmp, db);
                        if (iRet == DBResult.Success)
                        {
                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                string sDeviceID = Convert.ToString(dtTmp.Rows[i]["DeviceID"]);
                                string sCmdSno = Convert.ToString(dtTmp.Rows[i]["CMDSNO"]);
                                string strCompleteCode = Convert.ToString(dtTmp.Rows[i]["CompleteCode"]);
                                string sTaskMode = Convert.ToString(dtTmp.Rows[i]["TransferMode"]);
                                string sFinishLoc = Convert.ToString(dtTmp.Rows[i]["FinishLocation"]);
                                int iMode = int.Parse(sTaskMode);
                                string sSource = Convert.ToString(dtTmp.Rows[i]["Source"]);
                                int.TryParse(sSource, out int iSource);
                                string sDestination = Convert.ToString(dtTmp.Rows[i]["Destination"]);
                                int.TryParse(sDestination, out int iDest);

                                if (iMode == (int)clsEnum.TaskMode.Move)
                                {
                                    EQU_CMD.FunInsertHisEquCmd(sCmdSno, db);
                                    EQU_CMD.DeleteEquCmd(sCmdSno, db);
                                    continue;
                                }

                                //clsEnum.LocType locType_Dest;
                                //clsEnum.LocType locType_Source;
                                //if (iSource > 100) locType_Source = clsEnum.LocType.Shelf;
                                //else locType_Source = clsEnum.LocType.Port;

                                //if (iDest > 100) locType_Dest = clsEnum.LocType.Shelf;
                                //else locType_Dest = clsEnum.LocType.Port;

                                CmdMstInfo cmd = new CmdMstInfo();
                                if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db) == false)
                                {
                                    if (iRet == DBResult.NoDataSelect)
                                    {
                                        if (EQU_CMD.FunInsertHisEquCmd(sCmdSno, db))
                                        {
                                            EQU_CMD.DeleteEquCmd(sCmdSno, db);
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

        public GetDataResult GetEquCmd(string cmdSno, out DataObject<EquCmd> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.GetEquCmd(cmdSno, out dataObject, db);
                    }
                    else
                    {
                        dataObject = new DataObject<EquCmd>();
                        return GetDataResult.Initial;
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject = new DataObject<EquCmd>();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return GetDataResult.Exception;
            }
        }

        public GetDataResult GetEquCmdByOutMode(int craneNo, string destination, out DataObject<EquCmd> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.GetEquCmdByOutMode(craneNo, destination, out dataObject, db);
                    }
                    else
                    {
                        dataObject = new DataObject<EquCmd>();
                        return GetDataResult.Initial;
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject = new DataObject<EquCmd>();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return GetDataResult.Exception;
            }
        }

        public GetDataResult GetEquCmdByInMode(int craneNo, string source, out DataObject<EquCmd> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.GetEquCmdByInMode(craneNo, source, out dataObject, db);
                    }
                    else
                    {
                        dataObject = new DataObject<EquCmd>();
                        return GetDataResult.Initial;
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject = new DataObject<EquCmd>();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return GetDataResult.Exception;
            }
        }

        public GetDataResult GetEquCmdByLocToLoc(int craneNo, out DataObject<EquCmd> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.GetEquCmdByLocToLoc(craneNo, out dataObject, db);
                    }
                    else
                    {
                        dataObject = new DataObject<EquCmd>();
                        return GetDataResult.Initial;
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject = new DataObject<EquCmd>();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return GetDataResult.Exception;
            }
        }

        public bool FunDelHisEquCmd(double dblDay)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return EQU_CMD.FunDelHisEquCmd(dblDay, db);
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
