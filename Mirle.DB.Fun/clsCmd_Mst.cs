using System;
using Mirle.Def;
using System.Data;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;

namespace Mirle.DB.Fun
{
    public class clsCmd_Mst
    {
        private clsTool tool = new clsTool();

        #region Store In

        public GetDataResult GetCmdMstByStoreInStart(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations} '";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInCrane(string cmdsno, out DataObject<CmdMst> dataObject, SqlServer db) //同時處理盤點入庫
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CmdSno='{cmdsno}' ";
            sql += $"AND TRACE IN ('{Trace.StoreInWriteCmdToCV}','{Trace.StoreOutCreateCraneCmd}') "; 
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.StoreInCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (sql.EndsWith(","))
                {
                    sql += $" '{stn}'";
                }
                else
                {
                    sql += $"'{stn}',";
                }
            }
            sql += $")";
            return db.GetData(sql, out dataObject);
        }

        #endregion Store In



        #region Store Out

        public GetDataResult GetCmdMstByStoreOutStart(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations} '";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOutcheck(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT COUNT(CmdSno) as COUNT FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations} '";
            return db.GetData(sql, out dataObject);

        }

        public GetDataResult GetCmdMstByStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CmdSno='{CmdSno}' ";
            sql += $"AND TRACE='{Trace.StoreOutWriteCraneCmdToCV}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)//盤點出庫不要在這裡被更新到
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}','{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.StoreOutCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (sql.EndsWith(","))
                {
                    sql += $" '{stn}'";
                }
                else
                {
                    sql += $"'{stn}',";
                }
            }
            sql += $")";
            return db.GetData(sql, out dataObject);
        }

        #endregion Store Out




        #region Empty Store In

        public GetDataResult GetEmptyCmdMstByStoreIn(string cmdsno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}') ";
            sql += $"AND CMDSNO='{cmdsno}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreInWriteCraneCmdToCV}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreInCraneCmdFinish}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (sql.EndsWith(","))
                {
                    sql += $" '{stn}'";
                }
                else
                {
                    sql += $"'{stn}',";
                }
            }
            sql += $")";
            return db.GetData(sql, out dataObject);
        }

        #endregion  Empty Store In




        #region Empty Store Out

        public GetDataResult GetCmdMstByEmptyStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND CmdSno='{CmdSno}' ";
            sql += $"AND TRACE='{Trace.EmptyStoreOutWriteCraneCmdToCV}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreOutCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (sql.EndsWith(","))
                {
                    sql += $" '{stn}'";
                }
                else
                {
                    sql += $"'{stn}',";
                }
            }
            sql += $")";
            return db.GetData(sql, out dataObject);
        }

        #endregion Empty Store Out




        #region L2L

        public GetDataResult GetLocToLoc(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.L2L}') ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.GetData(sql, out dataObject);
        }

        #endregion  L2L




        #region Update

        public ExecuteSQLResult UpdateCmdMst(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMst(string cmdSno, string cmdSts, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}', ";
            sql += $"CMDSTS='{cmdSts}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CMDSTS='{clsConstValue.CmdSts.strCmd_Running}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace, int trayWeight, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CMDSTS='{clsConstValue.CmdSts.strCmd_Running}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"TRAYWEIGHT='{trayWeight}', ";
            sql += $"EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string REMARK, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        #endregion Update

       

        public bool FunGetCommand_byTaskNo(string taskNo, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST where TaskNo= '" + taskNo + "' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    cmd = tool.GetCommand(dtTmp);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
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
            finally
            {
                dtTmp = null;
            }
        }

        public bool FunInsCmdMst(CmdMstInfo stuCmdMst, ref string strErrMsg, SqlServer db)
        {
            string sSQL = "";
            try
            {
                sSQL = "INSERT INTO CMD_MST (CMDSNO, CMDSTS, PRT, Cmd_Abnormal, StnNo, CmdMode, Iotype, Loc, NewLoc,";
                sSQL += "CrtDate, ExpDate, EndDate, UserID, EquNO, taskNo) values(";
                sSQL += "'" + stuCmdMst.CmdSno + "', ";
                sSQL += "'" + clsConstValue.CmdSts.strCmd_Initial + "', ";
                sSQL += "'" + stuCmdMst.Prt + "', 'NA', ";
                sSQL += "'" + stuCmdMst.StnNo + "', ";
                sSQL += "'" + stuCmdMst.CmdMode + "', ";
                sSQL += "'" + stuCmdMst.IoType + "', ";
                sSQL += "'" + stuCmdMst.Loc + "', ";
                sSQL += "'" + stuCmdMst.NewLoc + "', ";
                sSQL += "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '', '', 'WCS', ";
                sSQL += "'" + stuCmdMst.EquNo + "', ";
                sSQL += "'" + stuCmdMst.taskNo + "')";

                if (db.ExecuteSQL(sSQL, ref strErrMsg) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(sSQL);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(sSQL + " => " + strErrMsg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        

        #region Micron Fun
        public int FunGetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"select * from CMD_MST where CMDSTS in ('{clsConstValue.CmdSts.strCmd_Cancel}', '{clsConstValue.CmdSts.strCmd_Finished}')";
                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet != DBResult.Success && iRet != DBResult.NoDataSelect)
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");

                return iRet;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return DBResult.Exception;
            }
        }

        public bool FunGetCommand(string sCmdSno, ref CmdMstInfo cmd, ref int iRet, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST where CMDSNO = '" + sCmdSno + "' ";
                iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    cmd = tool.GetCommand(dtTmp);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
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
            finally
            {
                dtTmp = null;
            }
        }

        public int FunGetCmdMst_Grid(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSql = $"select * from CMD_MST" +
                    $" where CMDSTS < '{clsConstValue.CmdSts.strCmd_Finished}' ";
                strSql += " ORDER BY PRT, CrtDate, CMDSNO";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet != DBResult.Success && iRet != DBResult.NoDataSelect)
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                }

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }

        public bool FunDelCmdMst(string sCmdSno, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSQL = "delete from CMD_MST where CMDSNO = '" + sCmdSno + "' ";
                int Ret = db.ExecuteSQL(strSQL, ref strEM);
                if (Ret == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSQL); return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSQL + " => " + strEM); return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunInsertCMD_MST_His(string sCmdSno, SqlServer db)
        {
            try
            {
                string SQL = "INSERT INTO CMD_MST_His ";
                SQL += $" SELECT '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}', * FROM CMD_MST ";
                SQL += $" WHERE CMDSNO='{sCmdSno}'";

                int iRet = db.ExecuteSQL(SQL);
                if (iRet == DBResult.Success)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDelCMD_MST_His(double dblDay, SqlServer db)
        {
            try
            {
                string strDelDay = DateTime.Today.Date.AddDays(dblDay * (-1)).ToString("yyyy-MM-dd");
                string strSql = "delete from CMD_MST_His where HisDT <= '" + strDelDay + "' ";

                int iRet = db.ExecuteSQL(strSql);
                if (iRet == DBResult.Success)
                {
                    return true;
                }
                else return false;
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
