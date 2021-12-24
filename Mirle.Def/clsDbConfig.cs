using Mirle.DataBase;

namespace Mirle.Def
{
    public class clsDbConfig : IDBConfig
    {
        public DBTypes DBType { get; set; }
        public string DbName { get; set; }
        public string DbPassword { get; set; }
        public int DbPort { get; set; }
        public string DbServer { get; set; }
        public string DbUser { get; set; }
        public string FODBServer { get; set; }
        public int CommandTimeOut { get; set; }
        public int ConnectTimeOut { get; set; }
        public bool WriteLog { get; set; }
    }
}
