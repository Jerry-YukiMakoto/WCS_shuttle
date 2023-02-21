using System;
using System.Collections.Generic;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using System.ComponentModel.Design;

namespace Mirle.DB.Fun
{
    public class clsTaskNo
    {

        public ExecuteSQLResult StorIninsertTaskstart(string DeviceID,int Cmdstate,string commanID,string TaskNo,string TaskState,string source,string destination, SqlServer db)
        {
            string sSQL = "";
            sSQL = "INSERT INTO Task (DeviceID,CMDState, CommandID, TaskNo, TaskState,InitialDT ,source,destination";
            sSQL += ") values(";
            sSQL += "'" + DeviceID + "', ";
            sSQL += "'" + Cmdstate + "', ";
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

        public ExecuteSQLResult UpdateTaskStateGolevel(string cmdSno, string level, SqlServer db)
        {
            string sql = "UPDATE Task ";
            sql += $"SET GoLevel='{level}', ";
            sql += $"ActiveDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CommandID='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateTaskStateSHCEnd(string CommandID, bool StoreIn, SqlServer db)
        {
            string cmdSts = "";
            if (StoreIn)
            {
                cmdSts = "7";
            }
            else
            {
                cmdSts = "1";
            }

            string sql = "UPDATE Task ";
            sql += $"SET CMDState='{cmdSts}', ";
            sql += $"ActiveDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"FinishDT='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"GoLevel='0' ";
            sql += $"WHERE CommandID='{CommandID}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateTaskforDouble_storage(string cmdSno, string trace,string newloc,string TaskNo,string Vehicle_ID ,SqlServer db)
        {
            string sql = "UPDATE Task ";
            sql += $"SET TaskState='{trace}', ";
            sql += $"Destination='{newloc}', ";
            sql += $"Vehicle_ID='{Vehicle_ID}', ";
            sql += $"TaskNo='{TaskNo}' ";
            sql += $"WHERE CommandID='{cmdSno}' ";
            sql += $"AND CMDState IN ('1') ";
            return db.ExecuteSQL2(sql);
        }

        public GetDataResult CheckTaskInsert(string CommandID, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Task ";
            sql += $"WHERE CMDState IN ('1') ";
            sql += $"AND CommandID IN ('{CommandID}') ";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult CheckTaskGolevel(string CommandID, out DataObject<CmdMst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Task ";
            sql += $"WHERE CMDState IN ('1') ";
            sql += $"AND CommandID IN ('{CommandID}') ";
            return db.GetData(sql, out dataObject);
        }
    }
}
