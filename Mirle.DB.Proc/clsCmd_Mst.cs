using System;
using System.Data;
using Mirle.Def;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;

namespace Mirle.DB.Proc
{
    public class clsCmd_Mst
    {
        private Fun.clsCmd_Mst CMD_MST = new Fun.clsCmd_Mst();
        private clsSno sno;
        private clsDbConfig _config = new clsDbConfig();
        public clsCmd_Mst(clsDbConfig config)
        {
            _config = config;
            sno = new clsSno(_config);
        }


        public GetDataResult GetCmdMstByStoreOutStart(string stations, out DataObject<CmdMst> dataObject) 
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreOutStart(stations, out dataObject,  db);
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

        public GetDataResult GetCmdMstByStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreOutCrane(CmdSno, out dataObject, db);
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

        public GetDataResult GetCmdMstByStoreOutcheck(string stations, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreOutcheck(stations, out dataObject, db);
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

        public GetDataResult GetCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreOutFinish(stations, out dataObject, db);
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

        public GetDataResult GetCmdMstByStoreInstart(string stations, out DataObject<CmdMst> dataObject) 
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreInstart(stations, out dataObject, db);
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

        public GetDataResult GetCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreInFinish(stations, out dataObject, db);
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

        public GetDataResult GetCmdMstByStoreInCrane(string cmdsno, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetCmdMstByStoreInCrane(cmdsno, out dataObject, db);
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

        public GetDataResult GetLocToLoc(out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetLocToLoc(out dataObject, db);
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

        public GetDataResult GetEmptyCmdMstByStoreIn(string cmdsno, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetEmptyCmdMstByStoreIn(cmdsno, out dataObject, db);
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

        public GetDataResult GetEmptyCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.GetEmptyCmdMstByStoreOutFinish(stations, out dataObject, db);
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

        //public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return CMD_MST.UpdateCmdMstTransferring(cmdSno, trace, db);
        //            }
        //            else
        //            {
        //                return ExecuteSQLResult.Initial;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return ExecuteSQLResult.Exception;
        //    }
        //}

        //public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string REMARK)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return CMD_MST.UpdateCmdMstRemark(cmdSno, REMARK, db);
        //            }
        //            else
        //            {
        //                return ExecuteSQLResult.Initial;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return ExecuteSQLResult.Exception;
        //    }
            
        //}

        //public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace, int trayWeight)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return CMD_MST.UpdateCmdMstTransferring(cmdSno, trace, trayWeight, db);
        //            }
        //            else
        //            {
        //                return ExecuteSQLResult.Initial;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return ExecuteSQLResult.Exception;
        //    }
            
        //}

        //public ExecuteSQLResult UpdateCmdMst(string cmdSno, string trace)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return CMD_MST.UpdateCmdMst(cmdSno, trace, db);
        //            }
        //            else
        //            {
        //                return ExecuteSQLResult.Initial;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return ExecuteSQLResult.Exception;
        //    }
            
        //}

        //public ExecuteSQLResult UpdateCmdMst(string cmdSno, string cmdSts, string trace)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return CMD_MST.UpdateCmdMst(cmdSno, cmdSts, trace, db);
        //            }
        //            else
        //            {
        //                return ExecuteSQLResult.Initial;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return ExecuteSQLResult.Exception;
        //    }
            
        //}

        public bool FunInsCmdMst(CmdMstInfo stuCmdMst, ref string strErrMsg)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.FunInsCmdMst(stuCmdMst, ref strErrMsg, db);
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

        
        #region Old Ver.

        public int FunGetCmdMst_Grid(ref DataTable dtTmp)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.FunGetCmdMst_Grid(ref dtTmp, db);
                    }
                    else return iRet;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }


        public bool FunMoveFinishCmdToHistory_Proc()
        {
            DataTable dtTmp = new DataTable();
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.FunGetFinishCommand(ref dtTmp, db) == DBResult.Success)
                        {
                            for (int i = 0; i < dtTmp.Rows.Count; i++)
                            {
                                string sCmdSno = Convert.ToString(dtTmp.Rows[i]["CmdSno"]);
                                string sRemark_Pre = Convert.ToString(dtTmp.Rows[i]["Remark"]);
                                string sRemark = "";
                                if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                                {
                                    sRemark = "Error: Begin失敗！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        CMD_MST.FunUpdateRemark(sCmdSno, sRemark, db);
                                    }

                                    continue;
                                }

                                if (!CMD_MST.FunInsertCMD_MST_His(sCmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    continue;
                                }

                                if (!CMD_MST.FunDelCmdMst(sCmdSno, db))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    continue;
                                }

                                db.TransactionCtrl(TransactionTypes.Commit);
                                return true;
                            }

                            return false;
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
            finally
            {
                dtTmp = null;
            }
        }

        public bool FunDelCMD_MST_His(double dblDay)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return CMD_MST.FunDelCMD_MST_His(dblDay, db);
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

        #endregion
    }
}
