using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Mirle.DataBase;

namespace Mirle.DB.Fun
{
    public class clsL2LCount
    {
        public int FunSelectNeedToTeach(int MaxCount, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"select * from L2LCount where DoCount >= {MaxCount}";
                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if(iRet == DBResult.Exception)
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

        public int CheckHasData(string BoxID, ref string strEM, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from L2LCount where BoxID = '{BoxID}' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);

                if (iRet == DBResult.Exception)
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
            finally
            {
                dtTmp = null;
            }
        }

        public bool FunUpdL2LCount(string BoxID, ref string strEM, SqlServer db)
        {
            try
            {
                string strSql = $"update L2LCount set DoCount = DoCount + 1, UpdDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' where BoxID = '{BoxID}' ";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
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

        public bool FunInsL2LCount(string BoxID, ref string strEM, SqlServer db)
        {
            try
            {
                string strSql = $"insert into L2LCount (BoxID, CrtDate) values('{BoxID}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
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

        public bool FunDelL2LCount(string BoxID, ref string strEM, SqlServer db)
        {
            try
            {
                string strSql = $"delete from L2LCount where BoxID = '{BoxID}' ";
                if (db.ExecuteSQL(strSql, ref strEM) == DBResult.Success)
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql);
                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
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
    }
}
