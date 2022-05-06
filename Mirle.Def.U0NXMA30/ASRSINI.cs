using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Net;

namespace Mirle.Def.U0NXMA30
{
    public interface ASRSINI
    {
        [Option(Alias = "Data Base")]
        DatabaseConfig Database { get; }

        [Option(Alias = "WMS Data Base")]
        DatabaseConfig Database_WMS { get; }

        [Option(Alias = "WMS API")]
        APIConfig WMS_API { get; }

        [Option(Alias = "WCS API")]
        APIConfig WCS_API { get; }

        [Option(Alias = "Device")]
        DeviceConfig Device { get; }

        [Option(Alias = "CV PLC")]
        CV_PlcConfig CV { get; }

        [Option(Alias = "StnNo")]
        StnNoConfig StnNo { get; }

        [Option(Alias = "CVerror")]
        CVError CVError { get; }
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
        [Option(DefaultValue = 0)]
        int MPLCNo { get; }
        string MPLCIP { get; }

        [Option(DefaultValue = 0)]
        int MPLCPort { get; }

        [Option(DefaultValue = 0)]
        int MPLCTimeout { get; }

        [Option(DefaultValue = 0)]
        int UseMCProtocol { get; }

        [Option(DefaultValue = 0)]
        int InMemorySimulator { get; }

    }

    

    public interface StnNoConfig
    {
        string A1 { get; }
        string A5 { get; }
        string A7 { get; }
        string A9 { get; }
        string WaterLevel { get; }
    }

    public interface CVError
    {
        string bit0 { get; }
        string bit1 { get; }
        string bit2 { get; }
        string bit3 { get; }
        string bit4 { get; }
        string bit5 { get; }
        string bit6 { get; }
        string bit7 { get; }
        string bit8 { get; }
        string bit9 { get; }
        string bitA { get; }
        string bitB { get; }
        string bitC { get; }
        string bitD { get; }
        string bitE { get; }
        string bitF { get; }

        string A2bit1 { get; }
        string A2bit2 { get; }
        string A2bit3 { get; }
        string A2bit4 { get; }
        string A2bit5 { get; }
        string A2bit6 { get; }
        string A2bit7 { get; }
        string A2bit8 { get; }

        string A4bit1 { get; }
        string A4bit3 { get; }
        string A4bit4 { get; }
        string A4bit5 { get; }
        string UPLVbit0 { get; }

    }
}
