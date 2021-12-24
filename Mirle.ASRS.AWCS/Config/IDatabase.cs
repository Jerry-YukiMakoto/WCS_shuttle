
using Config.Net;

namespace Mirle.ASRS.AWCS.Config
{
    public interface IDatabase
    {
        [Option(Alias = "DBType", DefaultValue = "OracleClient")] string DBType { get; set; }
        [Option(Alias = "DBServer")] string DBServer { get; set; }
        [Option(Alias = "DBPort")] int DBPort { get; set; }
        [Option(Alias = "DBName")] string DBName { get; set; }
        [Option(Alias = "DBUser")] string DBUser { get; set; }
        [Option(Alias = "DBPassword")] string DBPassword { get; set; }
        [Option(Alias = "CommandTimeOut")] int CommandTimeOut { get; set; }
        [Option(Alias = "ConnectTimeOut")] int ConnectTimeOut { get; set; }
        [Option(Alias = "WriteLog")] bool WriteLog { get; set; }
    }
}
