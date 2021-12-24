using System;

namespace Mirle.DataBase
{
    public sealed class TransactionCtrlResult : IEquatable<TransactionCtrlResult>
    {
        public int ResultCode { get; }
        public string Transaction { get; }
        public string OtherMessage { get; }

        public static TransactionCtrlResult Initial => new TransactionCtrlResult(DBResult.Initial);
        public static TransactionCtrlResult Success => new TransactionCtrlResult(DBResult.Success);
        public static TransactionCtrlResult Failed => new TransactionCtrlResult(DBResult.Failed);
        public static TransactionCtrlResult Exception => new TransactionCtrlResult(DBResult.Exception);

        private TransactionCtrlResult(int resultCode) : this(resultCode, string.Empty, string.Empty)
        {
        }
        private TransactionCtrlResult(int resultCode, string transaction) : this(resultCode, transaction, string.Empty)
        {
        }
        private TransactionCtrlResult(int resultCode, string transaction, string otherMessage)
        {
            ResultCode = resultCode;
            Transaction = transaction;
            OtherMessage = otherMessage;
        }

        internal static TransactionCtrlResult SetInitial() => new TransactionCtrlResult(DBResult.Initial);
        internal static TransactionCtrlResult SetBeginSuccess() => new TransactionCtrlResult(DBResult.Success, "Begin");
        internal static TransactionCtrlResult SetBeginFail() => new TransactionCtrlResult(DBResult.Failed, "Begin");
        internal static TransactionCtrlResult SetCommitSuccess() => new TransactionCtrlResult(DBResult.Success, "Commit");
        internal static TransactionCtrlResult SetCommitFail() => new TransactionCtrlResult(DBResult.Failed, "Commit");
        internal static TransactionCtrlResult SetRollbackSuccess() => new TransactionCtrlResult(DBResult.Success, "Rollback");
        internal static TransactionCtrlResult SetRollbackFail() => new TransactionCtrlResult(DBResult.Failed, "Rollback");
        internal static TransactionCtrlResult SetException(TransactionTypes transactionTypes, Exception ex) => new TransactionCtrlResult(DBResult.Exception, $"{nameof(transactionTypes)}", $"{ex}");

        public bool Equals(TransactionCtrlResult result)
        {
            return ResultCode == result?.ResultCode;
        }

        public override string ToString()
        {
            return Transaction;
        }
        public override int GetHashCode()
        {
            return ResultCode;
        }
        public override bool Equals(object obj)
        {
            if (obj is TransactionCtrlResult result)
            {
                return Equals(result);
            }
            else
            {
                return false;
            }
        }

        #region operator
        public static bool operator ==(TransactionCtrlResult result1, TransactionCtrlResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return true;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        public static bool operator !=(TransactionCtrlResult result1, TransactionCtrlResult result2)
        {
            if (ReferenceEquals(result1, result2))
                return false;
            else
                return (result1 ?? SetInitial()).Equals(result2);
        }
        #endregion operator
    }
}
