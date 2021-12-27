﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.ASRS.AWCS.Model.DataAccess;
using Mirle.ASRS.AWCS.Model.PLCDefinitions;
using Mirle.DataBase;

namespace Mirle.ASRS.AWCS.Manager
{
    public class DataAccessManger
    {
        private readonly DBOptions _dbOptions;
        private readonly LoggerManager _loggerManager;

        public DataAccessManger(CVCSHost host, DBOptions options)
        {
            _loggerManager = host.GetLoggerManager();
            _dbOptions = options;
        }

        public DB GetDB()
        {
            if (_dbOptions.DBType == DBTypes.OracleClient)
            {
                var db = new OracleClient(_dbOptions);
                db.Connect();
                return db;
            }
            else
            {
                var db = new SqlServer(_dbOptions);
                db.Connect();
                return db;
            }
        }

        public GetDataResult GetEmpMst(string stn, out DataObject<EmpMst> dataObject)
        {
            string sql = "";
            return GetDB().GetData(sql, out dataObject);
        }

        public GetDataResult GetCmdMstByStoreOut(string stations, out DataObject<CmdMst> dataObject)
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



        public GetDataResult GetCmdMstByStoreOut(IEnumerable<string> stations, string trayId, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}', '{3}') ";
                sql += $"AND TRAYID='{trayId}' ";
                sql += $"AND TRACE='{11}' ";
                sql += $"AND CMDSTS='{1}' ";
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
        public GetDataResult GetCmdMstByStoreIn(string cmdsno, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{1}', '{3}') ";
                sql += $"AND TRAYID='{cmdsno}' ";
                sql += $"AND TRACE IN ('{21}', '{22}', '{24}', '{26}') ";
                sql += $"AND CMDSTS='{1}' ";
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
                sql += $"AND TRACE IN ('{23}', '{25}') ";
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
        public GetDataResult GetCmdMstByStoreOutFinish(IEnumerable<string> stations, out DataObject<CmdMst> dataObject)
        {
            using (var db = GetDB())
            {
                string sql = "SELECT * FROM CMDMST ";
                sql += $"WHERE CMDMODE IN ('{2}', '{3}') ";
                sql += $"AND CMDSTS='{1}' ";
                sql += $"AND TRACE IN ('{12}', '{15}') ";
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

        public ExecuteSQLResult UpdateCmdMstTransferring(DB db, string cmdSno, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET CMDSTS='{1}', ";
            sql += $"TRACE='{trace}', ";
            sql += $"EXPTIME='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{0}' ";
            return db.ExecuteSQL2(sql);
        }
        public ExecuteSQLResult UpdateCmdMstTransferring(DB db, string cmdSno, string trace, int trayWeight)
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

        public ExecuteSQLResult UpdateCmdMst(DB db, string cmdSno, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{1}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult UpdateCmdMst(DB db, string cmdSno, string cmdSts, string trace)
        {
            string sql = "UPDATE CMDMST ";
            sql += $"SET TRACE='{trace}', ";
            sql += $"CMDSTS='{cmdSts}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{1}' ";
            return db.ExecuteSQL2(sql);
        }
        public ExecuteSQLResult UpdateCmdDtl(DB db, string cmdSno, string fosbId, string plant)
        {
            string sql = "UPDATE CMDDTL ";
            sql += $"SET FABID='{plant}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND LOTID='{fosbId}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult DeleteEquCmd(DB db, string cmdSno)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET RENEWFLAG='{"F"}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            sql += $"AND CMDSTS='{9}' ";
            sql += $"AND RENEWFLAG='{"Y"}' ";
            return db.ExecuteSQL2(sql);
        }
        public ExecuteSQLResult UpdateEquCmdRetry(DB db, string cmdSno)
        {
            string sql = "UPDATE EQUCMD ";
            sql += $"SET CMDSTS='{0}' ";
            sql += $"WHERE CMDSNO='{cmdSno}' ";
            return db.ExecuteSQL2(sql);
        }

        public ExecuteSQLResult InsertEquCmd(DB db, int craneNo, string cmdSno, string cmdMode, string source, string destination, int priority)
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

        public ExecuteSQLResult InsertCMD_MST(DB db, int craneNo, string cmdSno, string cmdMode, string source, string destination, int priority)
        {
            string sql = "INSERT INTO CMDMST (";
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
