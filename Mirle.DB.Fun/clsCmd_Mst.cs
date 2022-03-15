using System;
using Mirle.Def;
using System.Data;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;
using System.Linq;

namespace Mirle.DB.Fun
{
    public class clsCmd_Mst
    {
        private clsTool tool = new clsTool();

        #region Store In

        public GetDataResult GetCmdMstByStoreInStart(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations}'";
            sql += $"order by prt , crtdate , cmdsno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInCrane(string cmdsno, out DataObject<CmdMst> dataObject, SqlServer db) //同時處理盤點入庫
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CmdSno='{cmdsno}' ";
            sql += $"AND TRACE IN ('{Trace.StoreInWriteCmdToCV}','{Trace.StoreOutCreateCraneCmd}') "; 
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.StoreInCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (stations.Last() == stn)
                {
                    sql += $" '{stn}'";
                }
                else if (sql.EndsWith(","))
                {
                    sql += $" '{stn}',";
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
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations}'";
            sql += $"order by prt , crtdate , cmdsno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOutFinish(string cmdsno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.CMDFInish}') ";
            sql += $"AND Cmdsno='{cmdsno}' ";
            sql += $"AND Remark<>'{Remark.WMSReportComplete}' ";
            sql += $"AND Iotype =='{clsConstValue.IoType.NormalStockOut}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOutcheck(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT COUNT(CmdSno) as COUNT FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STNNO = '{stations}'";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByLOC(string sInsideLoc, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE LOC = '{sInsideLoc}'";
            sql += $"AND CmdSts IN ('{clsConstValue.CmdSts.strCmd_Initial}','{clsConstValue.CmdSts.strCmd_Running}') ";
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
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.StoreOutCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (stations.Last() == stn)
                {
                    sql += $" '{stn}'";
                }
                else if (sql.EndsWith(","))
                {
                    sql += $" '{stn}',";
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
            sql += $"AND CmdSno='{cmdsno}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreInWriteCraneCmdToCV}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"order by prt , crtdate , cmdsno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockIn}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreInCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (stations.Last() == stn)
                {
                    sql += $" '{stn}'";
                }
                else if (sql.EndsWith(","))
                {
                    sql += $" '{stn}',";
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
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.EmptyStoreOutCreateCraneCmd}') ";
            sql += $"AND STNNO IN (";
            foreach (var stn in stations)
            {
                if (stations.Last() == stn)
                {
                    sql += $" '{stn}'";
                }
                else if (sql.EndsWith(","))
                {
                    sql += $" '{stn}',";
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
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetLoctoLocFinish( out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST ";
            sql += $"WHERE CMDMODE IN ('{clsConstValue.CmdMode.L2L}') ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND TRACE IN ('{Trace.LoctoLocReady}') ";
            return db.GetData(sql, out dataObject);
        }

        #endregion  L2L




        #region Update

        public ExecuteSQLResult UpdateCmdMst(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}',";
            sql += $"Remark=''";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMst(string cmdSno, string cmdSts, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}', ";
            sql += $"CmdSts='{cmdSts}' ";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CmdSts='{clsConstValue.CmdSts.strCmd_Running}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"Remark='', ";
            sql += $"ExpDate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string cmdSts, string REMARK, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET EndDate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ,";
            sql += $"CmdSts='{cmdSts}' ,";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string REMARK, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET ExpDate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemarkandAbnormal(string cmdSno, string REMARK,string abnormal, SqlServer db)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET ExpDate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"EndDate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ,";
            sql += $"REMARK='{REMARK}' ,";
            sql += $"Cmd_Abnormal='{abnormal}' ";
            sql += $"WHERE CmdSno='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        #endregion Update



        public bool FunGetCommand_byTaskNo(string taskNo, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMDMST where CmdSno = '" + taskNo + "' ";
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
                sSQL = "INSERT INTO CMDMST (CmdSno, CmdSts, Cmd_Abnormal, Trace, StnNo, CmdMode, Iotype, whetherAllout, lastPallet, Loc, NewLoc,";
                sSQL += "CrtDate, ExpDate, EndDate, Remark, UserID, EquNo) values(";
                sSQL += "'" + stuCmdMst.CmdSno + "', ";
                sSQL += "'" + clsConstValue.CmdSts.strCmd_Initial + "', 'NA', '', ";
                sSQL += "'" + stuCmdMst.StnNo + "', ";
                sSQL += "'" + stuCmdMst.CmdMode + "', ";
                sSQL += "'" + stuCmdMst.IoType + "', ";
                sSQL += "'" + stuCmdMst.WhetherAllout + "', ";
                sSQL += "'" + stuCmdMst.lastPallet + "', ";
                sSQL += "'" + stuCmdMst.Loc + "', ";
                sSQL += "'" + stuCmdMst.NewLoc + "', ";
                sSQL += "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '', '', ";
                sSQL += "'WMS下命令', ";
                sSQL += "'" +stuCmdMst.Userid+"', ";
                sSQL += "'" + stuCmdMst.EquNo + "')";

                

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

        public GetDataResult EmptyInOutCheck(string Iotype,out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMDMST";
            sql += $"WHERE IOtype='{Iotype}' ";
            sql += $"AND CmdSts in ('{clsConstValue.CmdSts.strCmd_Initial}','{clsConstValue.CmdSts.strCmd_Running}') ";
            return db.GetData(sql, out dataObject);
        }

        #region Micron Fun
        public int FunGetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"select * from CMDMST where CmdSts in ('{clsConstValue.CmdSts.strCmd_Cancel}', '{clsConstValue.CmdSts.strCmd_Finished}')";
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
                string strSql = "select * from CMDMST where CmdSno = '" + sCmdSno + "' ";
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
                string strSql = $"select * from CMDMST" +
                    $" where CmdSts < '{clsConstValue.CmdSts.strCmd_Finished}' ";
                strSql += " ORDER BY CrtDate, CmdSno";
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
                string strSQL = "delete from CMDMST where CmdSno = '" + sCmdSno + "' ";
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
                SQL += $" SELECT '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}', * FROM CMDMST ";
                SQL += $" WHERE CmdSno='{sCmdSno}'";

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
