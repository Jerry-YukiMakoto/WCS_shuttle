using System;

namespace Mirle.DataBase
{
    public sealed class ExecuteSQLResult : IEquatable<ExecuteSQLResult>
    {
        public int ResultCode { get; }
        public string SqlScript { get; }
        public int AffectedRowCount { get; }
        public string OtherMessage { get; }

        public static ExecuteSQLResult Initial => new ExecuteSQLResult(DBResult.Initial);
        public static ExecuteSQLResult Success => new ExecuteSQLResult(DBResult.Success);
        public static ExecuteSQLResult NoDataUpdate => new ExecuteSQLResult(DBResult.NoDataUpdate);
        public static ExecuteSQLResult Exception => new ExecuteSQLResult(DBResult.Exception);

        public ExecuteSQLResult(int resultCode) : this(resultCode, string.Empty, 0)
        {
        }
        private ExecuteSQLResult(int resultCode, string sqlScript) : this(resultCode, sqlScript, 0)
        {
        }
        private ExecuteSQLResult(int resultCode, string sqlScript, int affectedRowCount)
        {
            ResultCode = resultCode;
            SqlScript = sqlScript;
            AffectedRowCount = affectedRowCount;
            OtherMessage = string.Empty;
        }
        private ExecuteSQLResult(int resultCode, string sqlScript, string otherMessage)
        {
            ResultCode = resultCode;
            SqlScript = sqlScript;
            OtherMessage = otherMessage;
        }

        internal static ExecuteSQLResult SetInitial() => new ExecuteSQLResult(DBResult.Initial);
        internal static ExecuteSQLResult SetSuccess(string sqlScript, int affectedRowCount) => new ExecuteSQLResult(DBResult.Success, sqlScript, affectedRowCount);
        internal static ExecuteSQLResult SetNoDataUpdate(string sqlScript) => new ExecuteSQLResult(DBResult.NoDataUpdate, sqlScript);
        internal static ExecuteSQLResult SetException(string sqlScript, Exception ex) => new ExecuteSQLResult(DBResult.Exception, sqlScript, $"{ex}");

        public bool Equals(ExecuteSQLResult result)
        {
            return ResultCode == result?.ResultCode;
        }

        public override string ToString()
        {
            return SqlScript;
        }
        public override int GetHashCode()
        {
            return ResultCode;
        }
        public override bool Equals(object obj)
        {
            if (obj is ExecuteSQLResult result)
            {
                return Equals(result);
            }
            else
            {
                return false;
            }
        }

        #region operator
        public static bool operator ==(ExecuteSQLResult result1, ExecuteSQLResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return true;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        public static bool operator !=(ExecuteSQLResult result1, ExecuteSQLResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return false;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        #endregion operator
    }
}
