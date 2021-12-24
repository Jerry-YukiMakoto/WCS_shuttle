using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DataBase
{
    public sealed class DBOptions
    {
        public DBTypes DBType { get; private set; }
        public string DBServer { get; private set; }
        public string FailoverDBServer { get; private set; }
        public int DBPort { get; private set; }
        public string DBName { get; private set; }
        public string DBUser { get; private set; }
        public string DBPassword { get; private set; }
        public int CommandTimeOut { get; private set; } = 30;
        public int ConnectTimeOut { get; private set; } = 10;
        public bool WriteLog { get; private set; } = false;
        public bool CheckConnection { get; private set; } = false;

        public DBOptions UseSqlserver()
        {
            DBType = DBTypes.SqlServer;
            return this;
        }
        public DBOptions UseOracleClient()
        {
            DBType = DBTypes.OracleClient;
            return this;
        }
        public DBOptions UseSQLite()
        {
            DBType = DBTypes.SQLite;
            return this;
        }
        public DBOptions SetDBType(DBTypes types)
        {
            DBType = types;
            return this;
        }

        public DBOptions SetDBServer(string dbServer)
        {
            return SetDBServer(dbServer, 0, string.Empty);
        }
        public DBOptions SetDBServer(string dbServer, int dbPort)
        {
            return SetDBServer(dbServer, dbPort, string.Empty);
        }
        public DBOptions SetDBServer(string dbServer, string failoverDBServer)
        {
            return SetDBServer(dbServer, 0, failoverDBServer);
        }
        public DBOptions SetDBServer(string dbServer, int dbPort, string failoverDBServer)
        {
            DBServer = dbServer;
            DBPort = dbPort;
            FailoverDBServer = failoverDBServer;
            return this;
        }

        public DBOptions SetAccount(string dbName)
        {
            return SetAccount(dbName, string.Empty, string.Empty);
        }
        public DBOptions SetAccount(string dbName, string dbUser, string dbPassword)
        {
            DBName = dbName;
            DBUser = dbUser;
            DBPassword = dbPassword;
            return this;
        }

        public DBOptions SetCommandTimeOut(int second)
        {
            CommandTimeOut = second;
            return this;
        }
        public DBOptions SetConnectTimeOut(int second)
        {
            ConnectTimeOut = second;
            return this;
        }

        public DBOptions EnableWriteLog()
        {
            WriteLog = true;
            return this;
        }
        public DBOptions EnableCheckConnection()
        {
            CheckConnection = true;
            return this;
        }
    }
}
