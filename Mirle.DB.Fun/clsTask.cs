using System;
using Mirle.Structure;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;
using Mirle.Micron.U2NMMA30;

namespace Mirle.DB.Fun
{
    public class clsTask
    {
        private clsSno SNO = new clsSno();
        private clsCmd_Mst CMD_MST = new clsCmd_Mst();
        public int GetForkPickupCmd_ForStockOut(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"select * from Task where TransferMode = {(int)clsEnum.TaskMode.Pickup} ";
                strSql += $" and CommandID in (select CmdSno from CMD_MST where CmdMode = '{clsConstValue.CmdMode.StockOut}')";
                string strEM = "";
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

        public int GetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = "select * from Task where TaskState > " + ((int)clsEnum.TaskState.Transferring).ToString();
                //strSql += $" and TransferMode <> {(int)clsEnum.TaskMode.Move} ";

                string strEM = "";
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

        public int GetForkCommand(int StockerID, int Fork, ref CmdMstInfo cmd, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSQL = "SELECT * FROM Task WHERE DeviceID='" + StockerID + "' ";
                strSQL += $" and ForkNo = {Fork} ";
                string strEM = "";
                int iRet = db.GetDataTable(strSQL, ref dtTmp, ref strEM);
                if(iRet == DBResult.Success)
                {
                    string sCmdSno = Convert.ToString(dtTmp.Rows[0]["CommandID"]);
                    if (CMD_MST.FunGetCommand(sCmdSno, ref cmd, ref iRet, db))
                    {
                        return iRet;
                    }
                    else throw new Exception($"<任務號>{sCmdSno} => 找不到系統命令");
                }
                else
                {
                    if (iRet != DBResult.NoDataSelect)
                        clsWriLog.Log.FunWriTraceLog_CV($"{strSQL} => {strEM}");

                    return iRet;
                }
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

        /// <summary>
        /// 確認沒有Task命令
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="sRemark"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool FunCheckNoTaskCmd(TaskDTO dto, ref string sRemark, SqlServer db)
        {
            int iRet = CheckHasTaskCmd(dto.CommandID, db);
            if (iRet == DBResult.Success)
            {
                sRemark = "Error: 該命令有設備命令正在執行！";
                if (sRemark != dto.Remark)
                {
                    CMD_MST.FunUpdateRemark(dto.CommandID, sRemark, db);
                }

                return false;
            }
            else
            {
                if (iRet != DBResult.NoDataSelect)
                {
                    sRemark = "Error: 檢查Task命令失敗！";
                    if (sRemark != dto.Remark)
                    {
                        CMD_MST.FunUpdateRemark(dto.CommandID, sRemark, db);
                    }

                    return false;
                }
            }

            return true;
        }

        public int CheckHasTaskCmd(string CommandID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = "select * from Task where CommandID = '" + CommandID + "' ";

                string strEM = "";
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
            finally
            {
                dtTmp = null;
            }
        }

        public int CheckHasTaskCmd(int StockerID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = "select * from Task where DeviceID = '" + StockerID + "' ";

                string strEM = "";
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
            finally
            {
                dtTmp = null;
            }
        }

        public bool FunCheckForkNoCmd(TaskDTO dto, ref string sRemark, SqlServer db)
        {
            if (funCheckTaskCmdRepeat(dto.StockerID, dto.ForkNo, db))
            {
                sRemark = $"Error: Stocker{dto.StockerID}的Fork{dto.ForkNo}還有命令正在執行！";
                if (sRemark != dto.Remark)
                {
                    CMD_MST.FunUpdateRemark(dto.CommandID, sRemark, db);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 檢查Stocker是否存在命令；存在命令傳回true，否則傳回false
        /// </summary>
        /// <param name="EquNo"></param>
        /// <param name="Fork"></param>
        /// <returns></returns>
        public bool funCheckTaskCmdRepeat(string EquNo, int Fork, SqlServer db)
        {
            DataTable objDataTable = new DataTable();
            int intCraneNo = 0;

            try
            {
                string strSQL = "SELECT COUNT (*) AS ICOUNT FROM Task WHERE DeviceID='" + EquNo + "' ";
                strSQL += $" and ForkNo = {Fork} ";

                objDataTable = new DataTable();
                if (db.GetDataTable(strSQL, ref objDataTable) == DBResult.Success)
                {
                    intCraneNo = int.Parse(objDataTable.Rows[0]["ICOUNT"].ToString());
                    return intCraneNo == 0 ? false : true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
            finally
            {
                if (objDataTable != null)
                {
                    objDataTable.Clear();
                    objDataTable.Dispose();
                    objDataTable = null;
                }
            }
        }

        public int FunSelectTaskCmd(string TaskNo, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from Task where TaskNo='{TaskNo}'";
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

        public int FunSelectTaskCmdByCommandID(string sCmdSno, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from Task where CommandID='{sCmdSno}'";
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

        public bool RepeatTaskCmd_Proc(CmdMstInfo cmd, SqlServer db)
        {
            try
            {
                string sTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSUO, db);
                if (string.IsNullOrWhiteSpace(sTaskNo))
                {
                    string sRemark = "Error: 取得TaskNo失敗！";
                    if (sRemark != cmd.Remark)
                    {
                        CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                    }

                    return false;
                }

                FunInsertHisTask(cmd.CmdSno, db);
                return FunRepeatTaskCmd(cmd.CmdSno, sTaskNo, db);
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunDelTaskCmd_Proc(CmdMstInfo cmd, SqlServer db)
        {
            try
            {
                FunInsertHisTask(cmd.CmdSno, db);
                return FunDelTaskCmd(cmd.CmdSno, db);
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool RepeatTaskCmd_ForToStockOutProc(CmdMstInfo cmd, int StockerID, int StkPort, SqlServer db)
        {
            try
            {
                var buffer = clsMicronCV.GetBufferByStkPort(StockerID, StkPort);
                FunInsertHisTask(cmd.CmdSno, db);
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

                if (!string.IsNullOrWhiteSpace(cmd.BatchID))
                {
                    if (!CMD_MST.FunCancelBatch(cmd.CmdMode, cmd.BatchID, db))
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        return false;
                    }
                }

                if (!FunDelTaskCmd(cmd.CmdSno, db))
                {
                    db.TransactionCtrl(TransactionTypes.Rollback);
                    return false;
                }

                var cv = clsMicronCV.GetConveyorController().GetBuffer(buffer.Index);
                if (cv.CommandID == cmd.CmdSno)
                {
                    if(!cv.NoticeInital().Result)
                    {
                        db.TransactionCtrl(TransactionTypes.Rollback);
                        sRemark = $"Error: <Buffer> {buffer.BufferName} => 通知PLC初始失敗！";
                        if (sRemark != cmd.Remark)
                        {
                            CMD_MST.FunUpdateRemark(cmd.CmdSno, sRemark, db);
                        }
                        return false;
                    }
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

        private bool FunRepeatTaskCmd(string CommandID, string sNewTaskNo, SqlServer db)
        {
            try
            {
                string strSql = $"update Task set TaskNo = '{sNewTaskNo}', QueueDT = CONVERT([VARCHAR], GETDATE(), (121))";
                strSql += $", TaskState = {(int)clsEnum.TaskState.Queue}, CompleteCode = '', FinishLocation = '' ";
                strSql += $" WHERE COMMANDID='{CommandID}' ";

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

        public int UpdateByTaskNo_ReturnInt(TaskDTO dto, SqlServer db)
        {
            var sql = $"update Task set TaskState={(int)dto.TaskState}" +
                $",CMDState={(int)dto.CMDState}" +
                $",AtSourceDT='{dto.AtSourceDT}'" +
                $",AtDestinationDT='{dto.AtDestinationDT}'" +
                $",BCRReadDT='{dto.BCRReadDT}'" +
                $",BCRReplyCSTID='{dto.BCRReplyCSTID}'" +
                $",BCRReadStatus={(int)dto.BCRReadStatus}" +
                $",InitialDT='{dto.InitialDT}'" +
                $",WaitingDT='{dto.WaitingDT}'" +
                $",ActiveDT='{dto.ActiveDT}'" +
                $",FinishDT='{dto.FinishDT}'" +
                $",FinishLocation='{dto.FinishLocation}'" +
                $",C1StartDT='{dto.C1StartDT}'" +
                $",CSTOnDT='{dto.CSTOnDT}'" +
                $",CSTTakeOffDT='{dto.CSTTakeOffDT}'" +
                $",C2StartDT='{dto.C2StartDT}'" +
                $",T1={dto.T1},T2={dto.T2},T3={dto.T3},T4={dto.T4}" +
                $",F1StartDT='{dto.F1StartDT}'" +
                $",F2StartDT='{dto.F2StartDT}'" +
                $",RenewFlag='Y' where TaskNo='{dto.TaskNo}'";

            string strEM = "";
            int iRet = db.ExecuteSQL(sql, ref strEM);
            if (iRet == DBResult.Success) clsWriLog.Log.FunWriTraceLog_CV(sql);
            else clsWriLog.Log.FunWriTraceLog_CV($"{sql} => {strEM}");

            return iRet;
        }

        public bool FunInsertTaskCmd(string CommandID, string TaskNo, string DeviceID, clsEnum.TaskMode mode, string sFrom, string sTo, ref string strEM, int Prt, string sBoxID, int iSpeed, int ForkNo, SqlServer db)
        {
            try
            {
                string strSql = "insert into Task(DeviceID, CommandID, TaskNo, Priority, TransferMode, Source, Destination, CSTID, BCRReadFlag, TravelAxisSpeed, LifterAxisSpeed, RotateAxisSpeed, ForkAxisSpeed, ForkNo) values (";
                strSql += "'" + DeviceID + "', '" + CommandID + "', '" + TaskNo + "', " + Prt.ToString() + ", ";
                strSql += (int)mode + ", '" + sFrom + "', '" + sTo + "', '" + sBoxID + "', ";
                strSql += $"'N', {iSpeed}, {iSpeed}, {iSpeed}, {iSpeed}, '{ForkNo}') ";
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

        public bool FunDelTaskCmd(string CommandID, SqlServer db)
        {
            try
            {
                string strEM = "";
                string strSQL = "delete from Task where CommandID = '" + CommandID + "' ";
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

        public bool FunInsertHisTask(string sCmdSno, SqlServer db)
        {
            try
            {
                string SQL = "INSERT INTO HisTask ";
                SQL += $" SELECT '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}', * FROM Task ";
                SQL += $" WHERE COMMANDID='{sCmdSno}'";

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

        public bool FunDelHisTask(double dblDay, SqlServer db)
        {
            try
            {
                string strDelDay = DateTime.Today.Date.AddDays(dblDay * (-1)).ToString("yyyy-MM-dd");
                string strSql = "delete from HisTask where HisDT <= '" + strDelDay + "' ";

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
