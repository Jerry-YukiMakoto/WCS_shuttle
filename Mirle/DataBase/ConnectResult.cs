using System;

namespace Mirle.DataBase
{
    public sealed class ConnectResult : IEquatable<ConnectResult>
    {
        public int ResultCode { get; }
        public string ConnectionString { get; }
        public string OtherMessage { get; }

        public static ConnectResult Initial => new ConnectResult(DBResult.Initial);
        public static ConnectResult Success => new ConnectResult(DBResult.Success);
        public static ConnectResult Fail => new ConnectResult(DBResult.Failed);
        public static ConnectResult Exception => new ConnectResult(DBResult.Exception);

        public ConnectResult(int resultCode) : this(resultCode, string.Empty, string.Empty)
        {
        }
        private ConnectResult(int resultCode, string connectionString) : this(resultCode, connectionString, string.Empty)
        {
        }
        private ConnectResult(int resultCode, string connectionString, string otherMessage)
        {
            ResultCode = resultCode;
            ConnectionString = connectionString;
            OtherMessage = otherMessage;
        }

        internal static ConnectResult SetInitial() => new ConnectResult(DBResult.Initial);
        internal static ConnectResult SetSuccess(string connectionString) => new ConnectResult(DBResult.Success, connectionString);
        internal static ConnectResult SetFail(string connectionString) => new ConnectResult(DBResult.Failed, connectionString);
        internal static ConnectResult SetException(string connectionString, Exception ex) => new ConnectResult(DBResult.Exception, connectionString, $"{ex}");

        public bool Equals(ConnectResult result)
        {
            return ResultCode == result?.ResultCode;
        }

        public override string ToString()
        {
            return ConnectionString;
        }
        public override int GetHashCode()
        {
            return ResultCode;
        }
        public override bool Equals(object obj)
        {
            if (obj is ConnectResult result)
            {
                return Equals(result);
            }
            else
            {
                return false;
            }
        }

        #region operator
        public static bool operator ==(ConnectResult result1, ConnectResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return true;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        public static bool operator !=(ConnectResult result1, ConnectResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return false;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        #endregion operator
    }
}
