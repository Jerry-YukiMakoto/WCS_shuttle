using System;

namespace Mirle.DataBase
{
    public sealed class GetDataResult : IEquatable<GetDataResult>
    {
        public int ResultCode { get; }
        public string SqlScript { get; }
        public string OtherMessage { get; }

        public static GetDataResult Initial => new GetDataResult(DBResult.Initial);
        public static GetDataResult Success => new GetDataResult(DBResult.Success);
        public static GetDataResult NoDataSelect => new GetDataResult(DBResult.NoDataSelect);
        public static GetDataResult Exception => new GetDataResult(DBResult.Exception);

        public GetDataResult(int resultCode) : this(resultCode, string.Empty, string.Empty)
        {
        }
        private GetDataResult(int resultCode, string sqlScript) : this(resultCode, sqlScript, string.Empty)
        {
        }
        private GetDataResult(int resultCode, string sqlScript, string otherMessage)
        {
            ResultCode = resultCode;
            SqlScript = sqlScript;
            OtherMessage = otherMessage;
        }

        internal static GetDataResult SetInitial() => new GetDataResult(DBResult.Initial);
        internal static GetDataResult SetSuccess(string sqlScript) => new GetDataResult(DBResult.Success, sqlScript);
        internal static GetDataResult SetNoDataSelect(string sqlScript) => new GetDataResult(DBResult.NoDataSelect, sqlScript);
        internal static GetDataResult SetException(string sqlScript, Exception ex) => new GetDataResult(DBResult.Exception, sqlScript, $"{ex}");

        public bool Equals(GetDataResult result)
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
            if (obj is GetDataResult result)
            {
                return Equals(result);
            }
            else
            {
                return false;
            }
        }

        #region operator
        public static bool operator ==(GetDataResult result1, GetDataResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return true;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        public static bool operator !=(GetDataResult result1, GetDataResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return false;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        #endregion operator
    }
}
