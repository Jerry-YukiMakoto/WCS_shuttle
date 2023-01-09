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
        public GetDataResult CheckCMDLifterOnlyONECMDatAtime(out DataObject<CmdMst> dataObject, SqlServer db)//在做有動到電梯的流程，要等整個流程結束才能執行下一個動電梯的命令流程，所以要先檢查
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE Cmd_sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"AND trace Not IN =('{Trace.StoreInWriteCmdToCV}','0') ";//入庫待機以及尚未執行的命令排除
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult CheckCMDLifterOnlyONECMDAtime(out DataObject<CmdMst> dataObject, SqlServer db)//在做有動到電梯的流程，要等整個流程結束才能執行下一個動電梯的命令流程，所以要先檢查
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE Cmd_sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult CheckCMDLifterOnlyONECMDatAtimeWith_SHCCALL(out DataObject<CmdMst> dataObject, SqlServer db)//在做有動到電梯的流程，要等整個流程結束才能執行下一個動電梯的命令流程，所以要先檢查
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE Cmd_sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"AND trace Not IN =('{Trace.StoreInWriteCmdToCV}','0') ";//入庫待機以及尚未執行的命令排除
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCMDPICKUpRunning(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE Cmd_sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"AND  CMD_MODE IN ('{clsConstValue.CmdMode.Cycle}') ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInStart(string Loc_ID,out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.StockIn}') ";
            sql += $"AND Cmd_Sts IN ('{clsConstValue.CmdSts.strCmd_Initial}') ";
            sql += $"AND Loc_ID='{Loc_ID}' ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByCycleIN(string Loc_ID, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND Cmd_Sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"AND Loc_ID='{Loc_ID}' ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreInLifterStart(string Sno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.StockIn}','{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Running}' ";
            sql += $"AND Cmd_Sno='{Sno}' ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }

        //public GetDataResult GetCmdMstByStoreInLifterToCarFloor(int floor, out DataObject<CmdMst> dataObject, SqlServer db)
        //{
        //    string sql = "SELECT * FROM CMD_MST ";
        //    sql += $"WHERE FLOOR IN {floor} ";
        //    sql += $"AND CmdSts='{clsConstValue.CmdSts.strCmd_Running}' ";
        //    sql += $"order by prt , crtdate , cmdsno";
        //    return db.GetData(sql, out dataObject);
        //}

        public GetDataResult GetCmdMstLifterCmd(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE Cmd_sts IN ('{clsConstValue.CmdSts.strCmd_Running}') ";
            sql += $"AND trace Not IN =('{Trace.StoreInWriteCmdToCV}','0') ";//入庫待機以及尚未執行的命令排除
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }


        #endregion Store In



        #region Store Out

        public GetDataResult GetCmdMstByPickUpStart(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.StockOut}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"order by prty , crt_date , cmd_sno";
            return db.GetData(sql, out dataObject);
        }



        public GetDataResult GetCmdMstByStoreOutcheck(string stations, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT COUNT(Cmd_Sno) as COUNT FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.StockOut}') ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"AND STN_NO = '{stations}'";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByLOC(string sInsideLoc, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE LOC = '{sInsideLoc}' ";
            sql += $"AND Cmd_Sts IN ('{clsConstValue.CmdSts.strCmd_Initial}','{clsConstValue.CmdSts.strCmd_Running}') ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.StockOut}', '{clsConstValue.CmdMode.Cycle}') ";
            sql += $"AND Cmd_Sno='{CmdSno}' ";
            sql += $"AND CMD_STS='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        #endregion Store Out









        #region L2L

        public GetDataResult GetLocToLoc(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.L2L}') ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetLoctoLocFinish(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM CMD_MST ";
            sql += $"WHERE CMD_MODE IN ('{clsConstValue.CmdMode.L2L}') ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.GetData(sql, out dataObject);
        }

        #endregion  L2L




        #region Update

        public ExecuteSQLResult UpdateCmdMstEnd(string cmdSno, string cmdSts, string trace, SqlServer db)
        {
            string sql = "UPDATE Cmd_Mst ";
            sql += $"SET TRACE='{trace}', ";
            sql += $"Cmd_Sts='{cmdSts}', ";
            sql += $"End_Date='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE Cmd_Sno='{cmdSno}' ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Running}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstTransferring(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE CMD_MST ";
            sql += $"SET Cmd_Sts='{clsConstValue.CmdSts.strCmd_Running}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"Remark='', ";
            sql += $"Exp_Date='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE Cmd_Sno='{cmdSno}' ";
            sql += $"AND Cmd_Sts='{clsConstValue.CmdSts.strCmd_Initial}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string cmdSts, string REMARK, SqlServer db)
        {
            string sql = "UPDATE CMD_MST ";
            sql += $"SET End_Date='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ,";
            sql += $"Cmd_Sts='{cmdSts}' ,";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE Cmd_Sno='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(string cmdSno, string REMARK, SqlServer db)
        {
            string sql = "UPDATE CMD_MST ";
            sql += $"SET Exp_Date='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE Cmd_Sno='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        #endregion Update





        public ExecuteSQLResult FunInsCmdMst(CmdMstInfo stuCmdMst, SqlServer db)
        {
            string sSQL = "";
                sSQL = "INSERT INTO CMD_MST (Cmd_Sno, Cmd_Sts, Cmd_Abnormal, Trace,prty ,Cmd_Mode, Io_type, Loc, New_Loc,";
                sSQL += "Crt_Date, Exp_Date, End_Date, Remark, Trn_User, Equ_No) values(";
                sSQL += "'" + stuCmdMst.CmdSno + "', ";
                sSQL += "'" + clsConstValue.CmdSts.strCmd_Initial + "', 'NA', '0', ";
                sSQL += "'" + stuCmdMst.Prt + "', ";
                sSQL += "'" + stuCmdMst.CmdMode + "', ";
                sSQL += "'" + stuCmdMst.IoType + "', ";
                sSQL += "'" + stuCmdMst.Loc + "', ";
                sSQL += "'" + stuCmdMst.NewLoc + "', ";
                sSQL += "'" + stuCmdMst.CrtDate + "', '', '', ";
                sSQL += "'WCS下命令', ";
                sSQL += "'" + stuCmdMst.TrnUser + "')";
                return db.ExecuteSQL2(sSQL);
         }

        public ExecuteSQLResult FunInsCmdDtl(struCmdDtl stuCmdDtl, SqlServer db)
        {
            string sSQL = "INSERT INTO Cmd_Dtl (Cmd_Txno, Cmd_Sno, Plt_Qty, Trn_Qty , In_Date, Item_No, Lot_No, LOC_Txno , TRN_Date ,CYCLE_Date, Tkt_NO, Store_CODE , BANK_CODE,";
            sSQL += "QC_CODE,Item_TYPE,expire_date ) values(";
            sSQL += "'" + stuCmdDtl.Cmd_Txno + "', ";
            sSQL += "'" + stuCmdDtl.Cmd_Sno + "', ";
            sSQL += "'" + stuCmdDtl.Plt_Qty + "', ";
            sSQL += "'" + stuCmdDtl.ALO_Qty + "', ";
            sSQL += "'" + stuCmdDtl.In_Date + "', ";
            sSQL += "'" + stuCmdDtl.Item_No + "', ";
            sSQL += "'" + stuCmdDtl.Lot_No + "', ";
            sSQL += "'" + stuCmdDtl.LOC_Txno + "', ";
            sSQL += "'" + stuCmdDtl.TRN_Date + "', ";
            sSQL += "'" + stuCmdDtl.CYCLE_Date + "', ";
            sSQL += "'" + stuCmdDtl.Tkt_NO + "', ";
            sSQL += "'" + stuCmdDtl.Store_CODE + "', ";
            sSQL += "'" + stuCmdDtl.BANK_CODE + "', ";
            sSQL += "'" + stuCmdDtl.QC_CODE + "', ";
            sSQL += "'" + stuCmdDtl.Item_TYPE+ "', ";
            sSQL += "'" + stuCmdDtl.expire_date + "')";
            return db.ExecuteSQL2(sSQL);
        }

        #region Micron Fun
        public int FunGetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strDelDay = DateTime.Today.Date.AddDays(1 * (-1)).ToString("yyyy-MM-dd");
                string strSql = $"select * from CMD_MST where Cmd_Sts in ('{clsConstValue.CmdSts.strCmd_Cancel}', '{clsConstValue.CmdSts.strCmd_Finished}')";
                strSql += $"And Crt_Date <=  '" + strDelDay + "'";
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
                string strSql = "select * from CMD_MST where Cmd_Sno = '" + sCmdSno + "' ";
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
                    $" where Cmd_Sts < '{clsConstValue.CmdSts.strCmd_Finished}' ";
                strSql += " ORDER BY Crt_Date, Cmd_Sno";
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
                string strSQL = "delete from CMD_MST where Cmd_Sno = '" + sCmdSno + "' ";
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
                SQL += $" SELECT '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', * FROM CMD_MST ";
                SQL += $" WHERE Cmd_Sno='{sCmdSno}'";

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
                string strSql = "delete from CMD_MST_His where CRT_DATE <= '" + strDelDay + "' ";

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
