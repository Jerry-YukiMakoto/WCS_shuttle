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

        //public ExecuteSQLResult DeleteEquCmd(string cmdSno)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return EQU_CMD.DeleteEquCmd(cmdSno, db);
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

        //public ExecuteSQLResult UpdateEquCmdRetry(string cmdSno)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return EQU_CMD.UpdateEquCmdRetry(cmdSno, db);
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

        //public ExecuteSQLResult InsertEquCmd(int craneNo, string cmdSno, string cmdMode, string source, string destination, int priority)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                return EQU_CMD.InsertEquCmd(craneNo, cmdSno, cmdMode, source, destination, priority, db);
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
