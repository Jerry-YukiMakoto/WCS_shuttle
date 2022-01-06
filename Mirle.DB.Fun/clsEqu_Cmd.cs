using System;
using Mirle.Def;
using System.Data;
using Mirle.Structure;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.Collections.Generic;

namespace Mirle.DB.Fun
{
    public class clsEqu_Cmd
    {
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
            string sql = "SELECT COUNT (*) AS COUNT FROM EQUCMD WHERE CMDSTS IN ('0', '1')";
            return db.GetData(sql, out dataObject);
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
            sql += "DeviceID, ";
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
            sql += $"WHERE DeviceID='{craneNo}' ";
            sql += $"AND CMDMODE ='{EquCmdMode.LocToLoc}' ";
            return db.GetData(sql, out dataObject);
        }

        public bool FunDelHisEquCmd(double dblDay, SqlServer db)
        {
            try
            {
                string strDelDay = DateTime.Today.Date.AddDays(dblDay * (-1)).ToString("yyyy-MM-dd");
                string strSql = "delete from HisEquCmd where HisDT <= '" + strDelDay + "' ";

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
                string SQL = "INSERT INTO HisEquCmd ";
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
