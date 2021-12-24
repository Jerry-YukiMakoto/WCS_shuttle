using System;
using System.Data;
using System.Data.SQLite;

namespace Mirle.DataBase
{
    public sealed class Sqlite : DB
    {
        private class DBConfig : IDBConfig
        {
            public DBTypes DBType { get; } = DBTypes.SQLite;
            public string DbServer { get; } = string.Empty;
            public string FODBServer { get; } = string.Empty;
            public int DbPort { get; } = 0;
            public string DbName { get; } = string.Empty;
            public string DbUser { get; } = string.Empty;
            public string DbPassword { get; } = string.Empty;
            public int MinPoolSize => 1;
            public int MaxPoolSize => 100;

            public int CommandTimeOut { get; } = 10;
            public int ConnectTimeOut { get; } = 30;
            public bool WriteLog { get; } = false;
            public DBConfig(string dbName)
            {
                DbName = dbName;
            }
        }

        [Obsolete("This Constructors is obsolete. Use Sqlite(string dbPath)", false)]
        public Sqlite(IDBConfig config) : base(config)
        {
        }

        public Sqlite(string dbPath) : base(new DBConfig(dbPath))
        {

        }

        public override bool CheckConnection()
        {
            if (_connection == null)
            {
                Open();
            }
            if (_connection.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override IDbConnection GetDbConnection()
        {
            return new SQLiteConnection(GetConnectionString());
        }

        public override IDbCommand GetDbCommand(string SQL, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand dbCommand = new SQLiteCommand(SQL, (SQLiteConnection)connection, (SQLiteTransaction)transaction);
            dbCommand.CommandTimeout = Config.CommandTimeOut;
            return dbCommand;
        }

        public override IDbCommand GetDbCommand(string SQL, IDbConnection connection)
        {
            IDbCommand dbCommand = new SQLiteCommand(SQL, (SQLiteConnection)connection);
            dbCommand.CommandTimeout = Config.CommandTimeOut;
            return dbCommand;
        }

        public override IDbDataAdapter GetDbDataAdapter()
        {
            return new SQLiteDataAdapter();
        }

        public override string GetConnectionString()
        {
            string strConnectString = $"Data Source={Config.DbName};Version=3;";
            return strConnectString;
        }
    }
}
