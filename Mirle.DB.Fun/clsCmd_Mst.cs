using System;
using Mirle.Def;
using System.Data;
using Mirle.Structure;
using Mirle.DataBase;
//using Mirle.Def.U2NMMA30;
//using Mirle.Micron.U2NMMA30;

namespace Mirle.DB.Fun
{
    public class clsCmd_Mst
    {
        private clsTool tool = new clsTool();

        public int CheckHasNeedL2LCmd(string Loc, ref string sCmdSno, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from CMD_MST where Loc = '{Loc}' and NeedShelfToShelf = '{clsEnum.NeedL2L.Y.ToString()}' ";
                strSql += $" and CmdSts < '{clsConstValue.CmdSts.strCmd_Finished}' ";

                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if(iRet == DBResult.Success)
                {
                    sCmdSno = Convert.ToString(dtTmp.Rows[0]["CmdSno"]);
                }
                else if(iRet != DBResult.NoDataSelect)
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    iRet = DBResult.Exception;
                }
                else { }

                return iRet;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public int FunGetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"select * from CMD_MST where CmdSts in ('{clsConstValue.CmdSts.strCmd_Cancel}', '{clsConstValue.CmdSts.strCmd_Finished}')";
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
                string strSql = "select * from CMD_MST where CmdSno = '" + sCmdSno + "' ";
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

        public bool FunGetCommand_ForPickupQuery(string sCmdSno, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST where CmdSno = '" + sCmdSno + "' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    cmd = tool.GetCommand(dtTmp);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
                    if(iRet == DBResult.NoDataSelect)
                    {
                        cmd = new CmdMstInfo(); dtTmp = new DataTable();
                        strSql = "select * from CMD_MST_His where CmdSno = '" + sCmdSno + "' ";
                        strSql += " order by HisDT desc";
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
                    else return false;
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

        public bool FunGetCommand_byJobId(string jobId, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST where JobID = '" + jobId + "' ";
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

        public int FunGetCommand_byBoxID(string sBoxID, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST where BoxId = '" + sBoxID + "' ";
                //strSql += $" and CmdMode = '{clsConstValue.CmdMode.StockOut}' and StnNo <> '' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    cmd = tool.GetCommand(dtTmp);
                }
                else
                {
                    if (iRet != DBResult.NoDataSelect)
                        clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
                }

                return iRet;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        

        public int FunGetCommandBatch_byCmdSno(string sCmdSno, ref string sBatchID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = "select BatchID from CMD_MST where CmdSno = '" + sCmdSno + "' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    sBatchID = Convert.ToString(dtTmp.Rows[0]["BatchID"]);
                }
                else if (iRet != DBResult.NoDataSelect)
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    iRet = DBResult.Exception;
                }
                else { }

                return iRet;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }


        public bool FunGetCmdMst(Location loc, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSql = "select * from CMD_MST";
                strSql += $" where CurDeviceID = '{loc.DeviceId}' and CurLoc = '{loc.LocationId}' ";
                if (db.GetDataTable(strSql, ref dtTmp, ref strEM) == DBResult.Success) return true;
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
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

        public int FunGetCmdMst_StockIn_L2L(int StockerID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = "select CmdSno from CMD_MST";
                strSql += $" where CmdSts < '{clsConstValue.CmdSts.strCmd_Finished}' " +
                    $"and (" +
                    $"(CmdMode = '{clsConstValue.CmdMode.StockIn}' and EquNO = '{StockerID}')" +
                    $" or (CmdMode = '{clsConstValue.CmdMode.L2L}' and SUBSTRING(NewLoc,1,2) in ";
                switch(StockerID)
                {
                    case 1:
                        strSql += " ('01','02','03','04'))";
                        break;
                    case 2:
                        strSql += " ('05','06','07','08'))";
                        break;
                    case 3:
                        strSql += " ('09','10','11','12'))";
                        break;
                    default:
                        strSql += " ('13','14'))";
                        break;
                }

                strSql += ") ";

                return db.GetDataTable(strSql, ref dtTmp);
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
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
                    $" where CmdSts < '{clsConstValue.CmdSts.strCmd_Finished}' ";
                strSql += " ORDER BY PRT, CrtDate, CmdSno";
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

        public int FunCheckHasCommand(string sLoc, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = $"select * from CMD_MST where Loc = '{sLoc}' or NewLoc = '{sLoc}' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    cmd = tool.GetCommand(dtTmp);
                }
                else
                {
                    if (iRet != DBResult.NoDataSelect)
                    {
                        clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    }
                }

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public int FunCheckHasCommand(string sLoc, string sCmdSts, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSql = $"select * from CMD_MST where Loc = '{sLoc}' or NewLoc = '{sLoc}' ";
                strSql += $" and CmdSts = '{sCmdSts}' ";
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

        public int FunCheckHasCommand(int iStockerID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from CMD_MST where CurDeviceID = '{iStockerID}' or EquNO = '{iStockerID}' ";
                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Exception)
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public bool FunUpdateRemark(string sCmdSno, string sRemark, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set Remark = N'" + sRemark + $"' where CmdSno = '{sCmdSno}'";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateStnNo(string sCmdSno, string sStnNo, string sRemark, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set StnNo = '" + sStnNo + $"', Remark = N'{sRemark}' ";
                strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdatePry(string sBoxID, string Pry, ref string strEM, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set PRT = '" + Pry + "' ";
                strSql += $" where BoxId = '{sBoxID}' ";

                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCmdSts(string sCmdSno, string sCmdSts, string sRemark, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set Remark = N'" + sRemark + $"', CmdSts = '{sCmdSts}' ";

                if(sCmdSts == clsConstValue.CmdSts.strCmd_Initial)
                {
                    strSql += ", CurLoc = '', CurDeviceID = '' ";
                }

                if (sCmdSts == clsConstValue.CmdSts.strCmd_Cancel || sCmdSts == clsConstValue.CmdSts.strCmd_Finished)
                {
                    strSql += ", EndDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                else
                {
                    strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCmdSts(string sCmdSno, string sCmdSts, string sStnNo, string sRemark, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set Remark = N'" + sRemark + $"', CmdSts = '{sCmdSts}' ";
                strSql += $", StnNo = '{sStnNo}' ";

                if (sCmdSts == clsConstValue.CmdSts.strCmd_Initial)
                {
                    strSql += ", CurLoc = '', CurDeviceID = '' ";
                }

                if (sCmdSts == clsConstValue.CmdSts.strCmd_Cancel || sCmdSts == clsConstValue.CmdSts.strCmd_Finished)
                {
                    strSql += ", EndDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                else
                {
                    strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCmdSts(string sCmdSno, string sCmdSts, clsEnum.Cmd_Abnormal abnormal, string sRemark, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set Remark = N'" + sRemark + $"', CmdSts = '{sCmdSts}' ";
                strSql += $", Cmd_Abnormal = '{abnormal.ToString()}' ";

                if (sCmdSts == clsConstValue.CmdSts.strCmd_Cancel || sCmdSts == clsConstValue.CmdSts.strCmd_Finished)
                {
                    strSql += ", EndDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                else
                {
                    strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateNewLocForL2L(string sCmdSno, string sNewLoc, SqlServer db)
        {
            try
            {
                string strSql = $"update CMD_MST set NewLoc = '{sNewLoc}' where CmdSno = '{sCmdSno}' ";
                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateLoc(string sCmdSno, string sLoc, string EquNO, SqlServer db)
        {
            try
            {
                string strSql = $"update CMD_MST set Loc = '{sLoc}', EquNO = '{EquNO}' where CmdSno = '{sCmdSno}' ";
                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCurLoc(string sCmdSno, string sCurDeviceID, string sCurLoc, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set CurDeviceID = '" + sCurDeviceID + $"', CurLoc = '{sCurLoc}', " +
                    $"CmdSts = '{clsConstValue.CmdSts.strCmd_Running}' ";

                strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCurLocAndCancelBatch(string sCmdSno, string sCurDeviceID, string sCurLoc, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set CurDeviceID = '" + sCurDeviceID + $"', CurLoc = '{sCurLoc}', " +
                    $"CmdSts = '{clsConstValue.CmdSts.strCmd_Running}' " + ", BatchId = '' ";

                strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCmd_ForTeachCmd(string sCmdSno, string sLoc, string sNewLoc, SqlServer db)
        {
            try
            {
                string strSql = $"update CMD_MST set Loc = '{sLoc}', NewLoc = '{sNewLoc}' ";
                strSql += $", CmdSts = '{clsConstValue.CmdSts.strCmd_Initial}', " +
                    $"CrtDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', ExpDate = '', EndDate= '' ";
                strSql += $", CurLoc = '', CurDeviceID = '' where CmdSno = '{sCmdSno}'";
                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateCurLocForS2S(string sCmdSno, string sCurDeviceID, string sCurLoc, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set CurDeviceID = '" + sCurDeviceID + $"', CurLoc = '{sCurLoc}', " +
                    $"CmdSts = '{clsConstValue.CmdSts.strCmd_Finished}' ";

                strSql += ", ExpDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunCancelBatch(string sCmdMode, string BatchID, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set BatchID = '' ";

                strSql += $" where CmdMode = '{sCmdMode}' and BatchID = '{BatchID}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunCancelBatch(string sCmdSno, SqlServer db)
        {
            try
            {
                string strSql = "update CMD_MST set BatchID = '' ";

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateNeedL2L(string sCmdSno, clsEnum.NeedL2L ans, SqlServer db)
        {
            try
            {
                string strSql = $"update CMD_MST set NeedShelfToShelf = '{ans.ToString()}' ";

                strSql += $" where CmdSno = '{sCmdSno}' ";

                string strEM = "";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
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
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunUpdateStnNo_ForCycleRun(SqlServer db)
        {
            string strEM = "";
            string strSql = $"update CMD_MST set StnNo = '1,2,3' where CmdMode = '{clsConstValue.CmdMode.StockOut}'";
            int iRet = db.ExecuteSQL(strSql, ref strEM);
            if (iRet == DBResult.Success) return true;
            else
            {
                if(iRet != DBResult.NoDataUpdate)
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                }
                return false;
            }
        }

        public bool FunInsCmdMst(CmdMstInfo stuCmdMst, ref string strErrMsg, SqlServer db)
        {
            string sSQL = "";
            try
            {
                sSQL = "INSERT INTO CMD_MST (CMDSNO, CmdSts, PRT, Cmd_Abnormal, StnNo, CmdMode, Iotype, Loc, NewLoc,";
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

        public bool FunDelCmdMst(string CommandID, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSQL = "delete from CMD_MST where CmdSno = '" + CommandID + "' ";
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
    }
}
