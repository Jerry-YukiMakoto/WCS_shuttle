using System;
using Mirle.Def;
using System.Data;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;
using Mirle.ASRS.WCS.Controller;

namespace Mirle.DB.Fun
{
    public class clsEqu_Cmd
    {
        /// <summary>
        /// 檢查是否有執行中命令；有執行中命令傳回true，否則傳回false
        /// </summary>
        public bool CheckExecutionEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, EquCmdMode equCmdMode, string source, string destination, SqlServer db)
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            if (GetEquCmd(cmdSno, out var equCmd, db) == GetDataResult.Success)
            {
                
                if (equCmd[0].CmdSts == CmdSts.Queue.ToString() || equCmd[0].CmdSts == CmdSts.Transferring.ToString())
                {
                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Exists Command On Equ Execute, Please Check => {cmdSno}, " +
                    $"{craneNo}, " +
                    $"{source}, " +
                    $"{destination}");

                    return true;
                }
                else
                {
                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Exists Command On Equ, Please Check => {cmdSno}, " +
                    $"{craneNo}, " +
                    $"{source}, " +
                    $"{destination}");

                    return true;
                }
            }
            else
            {
                if (checkCraneNoReapeat(out var dataObject, db) == GetDataResult.Success)
                {
                    int intCraneCount = 0;
                    intCraneCount = int.Parse(dataObject[0].COUNT.ToString());
                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Exists Other Command running On Equ, Please Check => {cmdSno}, " +
                   $"{craneNo}, " +
                   $"{source}, " +
                   $"{destination}");
                    return intCraneCount == 0 ? false : true;
                }
                else
                {
                    return true;
                }
            }
        }

        public int CheckHasEquCmd(string CmdSno, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = "select * from EQUCMD where CMDSNO = '" + CmdSno + "' ";

                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet != DBResult.Exception)
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
        public GetDataResult checkCraneNoReapeat(out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = $"SELECT COUNT (*) AS COUNT FROM EQUCMD WHERE CMDSTS IN ('{clsConstValue.CmdSts.strCmd_Initial}', '{clsConstValue.CmdSts.strCmd_Running}')";
            return db.GetData(sql, out dataObject);
        }

        public bool InsertStoreOutEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority, SqlServer db)
        {
            try
            {
                var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                if (source.Length != 7)
                {
                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Check Source Fail, Please Check => {cmdSno}, " +
                    $"{craneNo}, " +
                    $"{source}, " +
                    $"{destination}");

                    return false;
                }


                if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.OutMode, source, destination, db) == false)
                {
                    if (InsertEquCmd(craneNo, cmdSno, ((int)EquCmdMode.OutMode).ToString(), source, destination, priority, db) == ExecuteSQLResult.Success)
                    {
                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Insert Equ Cmd => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                        return true;
                    }
                    else
                    {
                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Insert Equ Cmd Fail=> {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                        return false;
                    }
                }
                else
                {
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
        }

        public bool InsertLocToLocEquCmd(int CmdType, string IoType, int craneNo, string cmdSno, string source, string destination, int priority, SqlServer db)
        {
            try
            {
                if (source.Length != 7)
                {
                    clsWriLog.L2LLogTrace(CmdType, IoType, $"Check Source Fail, Please Check => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                    return false;
                }

                if (destination.Length != 7)
                {
                    clsWriLog.L2LLogTrace(CmdType, IoType, $"Check Destination Fail, Please Check => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                    return false;
                }

                if (CheckExecutionEquCmd(CmdType, IoType, craneNo, cmdSno, EquCmdMode.LocToLoc, source, destination, db) == false)
                {
                    if (InsertEquCmd(craneNo, cmdSno, ((int)EquCmdMode.LocToLoc).ToString(), source, destination, priority, db) == ExecuteSQLResult.Success)
                    {
                        clsWriLog.L2LLogTrace(CmdType, IoType, $"Insert Equ Cmd => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                        return true;
                    }
                    else
                    {
                        clsWriLog.L2LLogTrace(CmdType, IoType, $"Insert Equ Cmd Fail => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                        return false;
                    }
                }
                else
                {
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
        }

        public bool InsertStoreInEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority, SqlServer db)
        {
            try
            {
                var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                if (destination.Length != 7)
                {
                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Check destination Fail, Please Check => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");

                    return false;
                }

                if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.InMode, source, destination, db) == false)
                {
                    if (InsertEquCmd(craneNo, cmdSno, ((int)EquCmdMode.InMode).ToString(), source, destination, priority, db) == ExecuteSQLResult.Success)
                    {
                        clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Insert Equ Cmd => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");
                        return true;
                    }
                    else
                    {
                        clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Insert Equ Cmd Fail => {cmdSno}, " +
                        $"{craneNo}, " +
                        $"{source}, " +
                        $"{destination}");
                        return false;
                    }
                }
                else
                {
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
        }

        public ExecuteSQLResult DeleteEquCmd(string cmdSno, SqlServer db)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET RENEWFLAG='{"F"}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{9}' ";
            sql += $"AND RENEWFLAG='{"Y"}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateEquCmdRetry(string cmdSno, SqlServer db)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET CMDSTS='{clsConstValue.CmdSts.strCmd_Initial}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult InsertEquCmd(int craneNo, string cmdSno, string cmdMode, string source, string destination, int priority, SqlServer db)
        {
            string sql = "INSERT INTO EQUCMD (";
            sql += "CMDSNO, ";
            sql += "EquNo, ";
            sql += "CMDMODE, ";
            sql += "CMDSTS, ";
            sql += "SOURCE, ";
            sql += "DESTINATION, ";
            sql += "LOCSIZE, ";
            sql += "PRIORITY, ";
            sql += "RCVDT ";
            sql += ") VALUES (";
            sql += $"'{cmdSno}', ";
            sql += $"'{craneNo}', ";
            sql += $"'{cmdMode}', ";
            sql += $"'{1}', ";
            sql += $"'{source}', ";
            sql += $"'{destination}', ";
            sql += $"'{0}', ";
            sql += $"'{priority}', ";
            sql += $"'{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";
            sql += $")";
            return db.ExecuteSQL2(sql);
        }

        public GetDataResult GetEquCmd(string cmdSno, out DataObject<EquCmd> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM EQUCMD ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.GetData(sql, out dataObject);
        }

        public int GetFinishCommand(ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = "select * from EQUCMD where TaskState > " + ((int)clsEnum.TaskState.Transferring).ToString();
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

        public GetDataResult GetEquCmdByOutMode(int craneNo, string destination, out DataObject<EquCmd> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM EQUCMD ";
            sql += $"WHERE DeviceID='{craneNo}' ";
            sql += $"AND CMDMODE IN ('{EquCmdMode.OutMode}', '{EquCmdMode.StnToStn}') ";
            sql += $"AND DESTINATION='{destination}'";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEquCmdByInMode(int craneNo, string source, out DataObject<EquCmd> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM EQUCMD ";
            sql += $"WHERE DeviceID='{craneNo}' ";
            sql += $"AND CMDMODE IN ('{EquCmdMode.InMode}', '{EquCmdMode.StnToStn}') ";
            sql += $"AND SOURCE='{source}'";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEquCmdByLocToLoc(int craneNo, out DataObject<EquCmd> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM EQUCMD ";
            sql += $"WHERE EquNo='{craneNo}' ";
            sql += $"AND CMDMODE ='{EquCmdMode.LocToLoc}' ";
            return db.GetData(sql, out dataObject);
        }

        public bool FunDelHisEquCmd(double dblDay, SqlServer db)
        {
            try
            {
                string strDelDay = DateTime.Today.Date.AddDays(dblDay * (-1)).ToString("yyyy-MM-dd");
                string strSql = "delete from EquCmdHis where HisDT <= '" + strDelDay + "' ";

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

        public bool FunInsertHisEquCmd(string sCmdSno, SqlServer db)
        {
            try
            {
                string SQL = "INSERT INTO EquCmdHis ";
                SQL += $" SELECT '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}', * FROM EQUCMD ";
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
    }
}
