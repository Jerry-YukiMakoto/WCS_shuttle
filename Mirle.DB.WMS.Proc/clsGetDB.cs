using System;
using System.Collections.Generic;
using System.Linq;

using Mirle.DataBase;
using Mirle.Def;

namespace Mirle.DB.WMS.Proc
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
            clsHost.IsConn = db.IsConnected;
            return iRet;
        }
    }
}
