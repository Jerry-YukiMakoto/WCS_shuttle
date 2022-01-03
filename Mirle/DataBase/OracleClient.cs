using System;
using System.Data;
using System.Reflection;

using Oracle.ManagedDataAccess.Client;
using Mirle.Def;

namespace Mirle.DataBase
{
    public sealed class OracleClient : DB
    {
        public OracleClient(DBOptions options) : base(options)
        {
        }
        [Obsolete]
        public OracleClient(clsDbConfig config) : base(config)
        {
        }


        //public override bool CheckConnection()
        //{
        //    if (_connection == null)
        //    {
        //        Open();
        //    }
        //    if (_connection.State == ConnectionState.Open)
        //    {
        //        try
        //        {
        //            using (var cmd = _connection.CreateCommand())
        //            {
        //                cmd.CommandText = "SELECT * FROM DUAL";
        //                cmd.ExecuteScalar();
        //                return true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteExceptionLog(MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public override IDbConnection GetDbConnection()
        {
            return new OracleConnection(GetConnectionString());
        }

        public override IDbCommand GetDbCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand dbCommand = new OracleCommand(sql, (OracleConnection)connection);
            dbCommand.CommandTimeout = Options.CommandTimeOut;
            dbCommand.Transaction = transaction;
            return dbCommand;
        }

        public override IDbCommand GetDbCommand(string sql, IDbConnection connection)
        {
            return GetDbCommand(sql, connection, null);
        }

        public override IDbDataAdapter GetDbDataAdapter()
        {
            return new OracleDataAdapter();
        }

        public override string GetConnectionString()
        {
            string strConnectString = $@"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={Options.DBServer})(PORT={Options.DBPort})))(CONNECT_DATA=(SERVICE_NAME={Options.DBName})));";
            strConnectString += $"Persist Security Info=True;";
            strConnectString += $"User ID={Options.DBUser};";
            strConnectString += $"Password={Options.DBPassword};";
            return strConnectString;
        }
    }
}
