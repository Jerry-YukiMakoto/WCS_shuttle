namespace Mirle.DataBase
{
    public interface IDBConfig
    {
        DBTypes DBType { get; }
        string DbServer { get; }
        string FODBServer { get; }
        int DbPort { get; }
        string DbName { get; }
        string DbUser { get; }
        string DbPassword { get; }
        int CommandTimeOut { get; }
        int ConnectTimeOut { get; }
        bool WriteLog { get; }
    }
}
