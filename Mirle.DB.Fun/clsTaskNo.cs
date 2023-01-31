using System;
using System.Collections.Generic;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;

namespace Mirle.DB.Fun
{
    public class clsTaskNo
    {

        public ExecuteSQLResult StorIninsertCMDstart(string DeviceID,string commanID,string TaskNo,string TaskState,string source,string destination, SqlServer db)
        {
            string sSQL = "";
            sSQL = "INSERT INTO Task (DeviceID, CommandID, TaskNo, TaskState,InitialDT ,source,destination";
            sSQL += ") values(";
            sSQL += "'" + DeviceID + "', ";
            sSQL += "'" + commanID + "', ";
            sSQL += "'" + TaskNo + "', ";
            sSQL += "'" + TaskState + "', ";
            sSQL += $"'{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sSQL += "'" + source + "', ";
            sSQL += "'" + destination + "') ";
            return db.ExecuteSQL2(sSQL);
        }

        public ExecuteSQLResult UpdateTaskState(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE Task ";
            sql += $"SET TaskState='{trace}', ";
            sql += $"ActiveDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CommandID='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateTaskStateEnd(string cmdSno, string trace, SqlServer db)
        {
            string sql = "UPDATE Task ";
            sql += $"SET TaskState='{trace}', ";
            sql += $"ActiveDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"FinishDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CommandID='{cmdSno}' ";
            sql += $"AND TaskState='{Trace.StoreInWCScommandReportSHC}' ";
            return db.ExecuteSQL2(sql);
        }

    }
}
