using Mirle.Def;
using Mirle.DataBase;

namespace Mirle.DB.Proc
{
    public class clsGetDB
    {
        public static SqlServer GetDB(clsDbConfig _config)
        {
            DBOptions options = new DBOptions();
            options.SetDBType(DBTypes.SqlServer);
            options.SetAccount(_config.DbName, _config.DbUser, _config.DbPassword);
            options.SetCommandTimeOut(_config.CommandTimeOut);
            options.SetConnectTimeOut(_config.ConnectTimeOut);
            options.SetDBServer(_config.DbServer, _config.DbPort, _config.FODBServer);
            options.EnableWriteLog();
            var db = new SqlServer(options);
            return db;
        }

        public static int FunDbOpen(SqlServer db)
        {
            string strEM = "";
            return FunDbOpen(db, ref strEM);
        }

        public static int FunDbOpen(SqlServer db, ref string strEM)
        {
            int iRet = db.Open(ref strEM);
            db.CheckConnection();
            clsHost.IsConn = db.IsConnected;
            if(iRet != DBResult.Success)
            {
                clsWriLog.Log.FunWriTraceLog_CV($"資料庫開啟失敗！=> {strEM}");
            }
            else
            {
                clsHost.IsConn = true;
            }

            return iRet;
        }
    }
}
