using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Net;

namespace Mirle.Def.T26YGAP0
{
    public interface ASRSINI
    {
        [Option(Alias = "Data Base")]
        DatabaseConfig Database { get; }

        //[Option(Alias = "WMS Data Base")]
        //DatabaseConfig Database_WMS { get; }

        //[Option(Alias = "WMS API")]
        //APIConfig WMS_API { get; }

        //[Option(Alias = "WCS API")]
        //APIConfig WCS_API { get; }

        [Option(Alias = "Device")]
        DeviceConfig Device { get; }

        [Option(Alias = "CV PLC")]
        CV_PlcConfig CV { get; }

        [Option(Alias = "SuttleStnNo")]
        StnNoConfig StnNo_Suttle { get; }

        [Option(Alias = "StnNo")]
        StnNoConfig StnNo { get; }
    }

    public interface DatabaseConfig
    {
        /// <summary>
        /// MSSQL, Oracle_Oledb, DB2, Odbc, Access, OleDb, Oracle_DB.enuDatabaseType.Oracle_OracleClient, SQLite,
        /// </summary>
        string DBMS { get; }
        string DbServer { get; }
        string FODbServer { get; }
        string DataBase { get; }
        string DbUser { get; }
        string DbPswd { get; }

        [Option(DefaultValue = 1433)]
        int DBPort { get; }

        [Option(DefaultValue = 30)]
        int ConnectTimeOut { get; }

        [Option(DefaultValue = 30)]
        int CommandTimeOut { get; }
    }

    public interface APIConfig
    {
        string IP { get; }
    }

    public interface DeviceConfig
    {
        string CraneID { get; }
        string Speed { get; }
    }


    public interface CV_PlcConfig
    {
        string IP { get; }

        [Option(DefaultValue = "")]
        string SHC_IP  { get; }

        [Option(DefaultValue = 0)]
        int SHC_PORT { get; }

        [Option(DefaultValue = "")]
        string BCR_IP_1 { get; }

        [Option(DefaultValue = "")]
        string BCR_IP_2 { get; }

        [Option(DefaultValue = "")]
        string BCR_port { get; }

    }



    public interface StnNoConfig
    {
        string STNL { get; }
        string STNR { get; }
    }

    
}
