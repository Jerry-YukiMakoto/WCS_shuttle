using System;
using System.Data;
using System.Data.SqlClient;
using Mirle.Def;

namespace Mirle.DataBase
{
    public sealed class SqlServer : DB
    {
        public SqlServer(DBOptions options) : base(options)
        {
        }

        [Obsolete]
        public SqlServer(clsDbConfig config) : base(config)
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
        //                cmd.CommandText = "SELECT 'AA'";
        //                cmd.ExecuteScalar();
        //                return true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteExceptionLog(System.Reflection.MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}");
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
            return new SqlConnection(GetConnectionString());
        }

        public override IDbCommand GetDbCommand(string SQL, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand dbCommand = new SqlCommand(SQL, (SqlConnection)connection, (SqlTransaction)transaction);
            dbCommand.CommandTimeout = Options.CommandTimeOut;
            return dbCommand;
        }

        public override IDbCommand GetDbCommand(string SQL, IDbConnection connection)
        {
            IDbCommand dbCommand = new SqlCommand(SQL, (SqlConnection)connection);
            dbCommand.CommandTimeout = Options.CommandTimeOut;
            return dbCommand;
        }

        public override IDbDataAdapter GetDbDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public override string GetConnectionString()
        {
            string strConnectString = $"Initial Catalog={Options.DBName};";
            strConnectString += $"Password={Options.DBPassword};";
            strConnectString += $"User ID={Options.DBUser};";
            strConnectString += $"Data Source={Options.DBServer};";
            strConnectString += $"Connect Timeout={Options.ConnectTimeOut};";
            strConnectString += $"MultipleActiveResultSets=True";
            return strConnectString;
        }
    }
}
