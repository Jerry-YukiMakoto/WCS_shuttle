using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Config.Net;
using Mirle.ASRS.AWCS.Config;
using Mirle.DataBase;

namespace Mirle.ASRS.AWCS
{
    public class CVCSHost
    {
        
        public CVCSHost()
        {
            var ini = new ConfigurationBuilder<IWCS>().UseIniFile("Config\\WCS.ini").Build();

            var dbOptions = new DBOptions();
            if (ini.Database.DBType == "OracleClient")
            {
                dbOptions.SetDBType(DBTypes.OracleClient);
            }
            else
            {
                dbOptions.SetDBType(DBTypes.SqlServer);
            }
            dbOptions.SetDBServer(ini.Database.DBServer, ini.Database.DBPort);
            dbOptions.SetAccount(ini.Database.DBName, ini.Database.DBUser, ini.Database.DBPassword);
            dbOptions.SetConnectTimeOut(ini.Database.ConnectTimeOut);
            if (ini.Database.WriteLog)
            {
                dbOptions.EnableWriteLog();
            }

           

        }

    }
}
