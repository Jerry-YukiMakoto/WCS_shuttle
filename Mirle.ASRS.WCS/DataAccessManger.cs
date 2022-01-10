using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.DataBase;
using Mirle.Def;

namespace Mirle.ASRS.WCS.Controller
{
    public class DataAccessManger
    {
        private readonly clsDbConfig _dbConfig;
        private readonly LoggerManager _loggerManager;

        public DataAccessManger(clsDbConfig dbConfig)
        {
            _loggerManager = ControllerReader.GetLoggerManager();
            _dbConfig = dbConfig;
        }

        public DataBase.DB GetDB()
        {

            if (_dbConfig.DBType == DBTypes.OracleClient)
            {
                var db = new OracleClient(_dbConfig);
                db.Connect();
                return db;
            }
            else
            {
                var db = new SqlServer(_dbConfig);
                db.Connect();
                return db;
            }
        }
        public GetDataResult GetCmdMstByStoreOutStart(string stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}', '{3}') ";
                sql += $"AND CMDSTS='{0}' ";
                sql += $"AND STNNO = '{stations} '";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}', '{3}') ";
                sql += $"AND CmdSno='{CmdSno}' ";
                sql += $"AND TRACE='{11}' ";
                sql += $"AND CMDSTS='{1}' ";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByEmptyStoreOutCrane(string CmdSno, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}') ";
                sql += $"AND CmdSno='{CmdSno}' ";
                sql += $"AND TRACE='{31}' ";
                sql += $"AND CMDSTS='{1}' ";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByStoreOutcheck(string stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT COUNT(CmdSno) as COUNT FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}') ";
                sql += $"AND CMDSTS='{0}' ";
                sql += $"AND STNNO = '{stations} '";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}','{3}') ";
                sql += $"AND CMDSTS='{1}' ";
                sql += $"AND TRACE IN ('{12}') ";
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
        }

        public GetDataResult GetCmdMstByStoreInCrane(string cmdsno, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}', '{3}') ";
                sql += $"AND CmdSno='{cmdsno}' ";
                sql += $"AND TRACE IN ('{21}') ";
                sql += $"AND CMDSTS='{1}' ";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByStoreInstart(string stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}', '{3}') ";
                sql += $"AND CMDSTS='{0}' ";
                sql += $"AND STNNO = '{stations} '";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}', '{3}') ";
                sql += $"AND CMDSTS='{1}' ";
                sql += $"AND TRACE IN ('{23}') ";
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
        }


        public GetDataResult checkCraneNoReapeat(out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT COUNT (*) AS COUNT FROM EQUCMD WHERE CMDSTS IN ('0', '1')";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetLocToLoc(out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{5}') ";
                sql += $"AND CMDSTS='{0}' ";
                return db.GetData(sql, out dataObject);
            }
        }


        public GetDataResult GetEmptyCmdMstByStoreIn(string cmdsno, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}') ";
                sql += $"AND CmdSno='{cmdsno}' ";
                sql += $"AND TRACE IN ('{41}') ";
                sql += $"AND CMDSTS='{1}' ";
                return db.GetData(sql, out dataObject);
            }
        }

        public GetDataResult GetEmptyCmdMstByStoreInFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}') ";
                sql += $"AND CMDSTS='{1}' ";
                sql += $"AND TRACE IN ('{43}') ";
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
        }
        
        public GetDataResult GetEmptyCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}') ";
                sql += $"AND CMDSTS='{1}' ";
                sql += $"AND TRACE IN ('{32}') ";
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
        }
        
        public ExecuteSQLResult UpdateCmdMstTransferring(DataBase.DB db, string cmdSno, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CMDSTS='{1}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{0}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMstRemark(DataBase.DB db, string cmdSno, string REMARK)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}', ";
            sql += $"REMARK='{REMARK}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }
        public ExecuteSQLResult UpdateCmdMstTransferring(DataBase.DB db, string cmdSno, string trace, int trayWeight)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CMDSTS='{1}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"TRAYWEIGHT='{trayWeight}', ";
            sql += $"EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{0}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMst(DataBase.DB db, string cmdSno, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{1}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMst(DataBase.DB db, string cmdSno, string cmdSts, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}', ";
            sql += $"CMDSTS='{cmdSts}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{1}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult DeleteEquCmd(DataBase.DB db, string cmdSno)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET RENEWFLAG='{"F"}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{9}' ";
            sql += $"AND RENEWFLAG='{"Y"}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateEquCmdRetry(DataBase.DB db, string cmdSno)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET CMDSTS='{0}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult InsertEquCmd(DataBase.DB db, int craneNo, string cmdSno, string cmdMode, string source, string destination, int priority)
        {
            string sql = "INSERT INTO EQUCMD (";
            sql += "CMDSNO, ";
            sql += "EQUNO, ";
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


        public GetDataResult GetEquCmd(string cmdSno, out DataObject<EquCmd> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM EQUCMD ";
                sql += $"WHERE CMDSNO='{cmdSno}' ";
                return db.GetData(sql, out dataObject);
            }
        }
        public GetDataResult GetEquCmdByOutMode(int craneNo, string destination, out DataObject<EquCmd> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM EQUCMD ";
                sql += $"WHERE EQUNO='{craneNo}' ";
                sql += $"AND CMDMODE IN ('{EquCmdMode.OutMode}', '{EquCmdMode.StnToStn}') ";
                sql += $"AND DESTINATION='{destination}'";
                return db.GetData(sql, out dataObject);
            }
        }
        public GetDataResult GetEquCmdByInMode(int craneNo, string source, out DataObject<EquCmd> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM EQUCMD ";
                sql += $"WHERE EQUNO='{craneNo}' ";
                sql += $"AND CMDMODE IN ('{EquCmdMode.InMode}', '{EquCmdMode.StnToStn}') ";
                sql += $"AND SOURCE='{source}'";
                return db.GetData(sql, out dataObject);
            }
        }
        public GetDataResult GetEquCmdByLocToLoc(int craneNo, out DataObject<EquCmd> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM EQUCMD ";
                sql += $"WHERE EQUNO='{craneNo}' ";
                sql += $"AND CMDMODE ='{EquCmdMode.LocToLoc}' ";
                return db.GetData(sql, out dataObject);
            }
        }
    }
}
