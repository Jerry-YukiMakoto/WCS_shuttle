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
            var db = new SqlServer(_config);
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
