using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using Mirle.Logger;

namespace Mirle.DataBase
{
    public abstract class DB : IDisposable
    {
        protected readonly object _Lock = new object();
        protected readonly Log _log = new Log();

        protected IDbConnection _connection;
        protected IDbTransaction _transaction;

        public DBOptions Options { get; }
        public IDBConfig Config { get; }

        [Obsolete]
        protected DB(IDBConfig config)
        {
            Config = config;
        }
        protected DB(DBOptions options)
        {
            Options = options;
        }

        public abstract string GetConnectionString();
        public abstract IDbConnection GetDbConnection();
        public abstract IDbCommand GetDbCommand(string SQL, IDbConnection connection, IDbTransaction transaction);
        public abstract IDbCommand GetDbCommand(string SQL, IDbConnection connection);
        public abstract IDbDataAdapter GetDbDataAdapter();

        [Obsolete]
        public int Open()
        {
            string strEM = string.Empty;
            return Open(ref strEM);
        }
        [Obsolete]
        public int Open(ref string errorMsg)
        {
            int intRetCode = DBResult.Initial;

            try
            {
                lock (_Lock)
                {
                    if (_connection == null)
                    {
                        _connection = GetDbConnection();
                    }

                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                        _connection.Dispose();
                        _connection = GetDbConnection();
                    }

                    _connection.Open();
                    if (_connection.State == ConnectionState.Open)
                    {
                        intRetCode = DBResult.Success;
                    }
                    else
                    {
                        errorMsg = "Open Fail";
                        intRetCode = DBResult.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                intRetCode = DBResult.Exception;
                WriteExceptionLog(ex);
                WriteSqlScript($"Open: {GetConnectionString()}{Environment.NewLine}OpenResult: Exception");
            }
            return intRetCode;
        }

        public ConnectResult Connect()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = GetDbConnection();
                }

                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                    _connection = GetDbConnection();
                }

                _connection.Open();
                if (_connection.State == ConnectionState.Open)
                {
                    WriteSqlScript($"Connect: {GetConnectionString()}{Environment.NewLine}ConnectResult: Success");
                    return ConnectResult.SetSuccess(GetConnectionString());
                }
                else
                {
                    WriteSqlScript($"Connect: {GetConnectionString()}{Environment.NewLine}ConnectResult: Fail");
                    return ConnectResult.SetFail(GetConnectionString());
                }
            }
            catch (Exception ex)
            {
                WriteExceptionLog(ex);
                WriteSqlScript($"Connect: {GetConnectionString()}{Environment.NewLine}ConnectResult: Exception");
                return ConnectResult.SetException(GetConnectionString(), ex);
            }
        }

        public ConnectResult ReConnect()
        {
            Close();
            return Connect();
        }

        public void Close()
        {
            WriteSqlScript($"Close");
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }

        [Obsolete]
        public int ExecuteSQL(string SQL)
        {
            string errorMsg = string.Empty;
            return ExecuteSQL(SQL, ref errorMsg);
        }

        [Obsolete]
        public int ExecuteSQL(string sqlScript, ref int executeCount)
        {
            string errorMsg = string.Empty;
            return ExecuteSQL(sqlScript, ref executeCount, ref errorMsg);
        }

        [Obsolete]
        public int ExecuteSQL(string sqlScript, ref string errorMsg)
        {
            int executeCount = 0;
            return ExecuteSQL(sqlScript, ref executeCount, ref errorMsg);
        }

        [Obsolete]
        public virtual int ExecuteSQL(string sqlScript, ref int executeCount, ref string errorMsg)
        {
            int intRetCode = DBResult.Initial;

            try
            {
                lock (_Lock)
                {
                    if (_connection is null)
                    {
                        _connection = GetDbConnection();
                    }

                    if (_connection.State == ConnectionState.Closed)
                    {
                        _connection.Open();
                    }

                    using (var command = GetDbCommand(sqlScript, _connection, _transaction))
                    {
                        executeCount = command.ExecuteNonQuery();
                        if (executeCount <= 0)
                        {
                            intRetCode = DBResult.NoDataUpdate;
                            errorMsg = "No Data Update";
                        }
                        else
                        {
                            intRetCode = DBResult.Success;
                            errorMsg = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                intRetCode = DBResult.Exception;
                WriteExceptionLog(ex, $"ExecuteSQL: {sqlScript}");
                WriteSqlScript($"ExecuteSQL: {sqlScript}{Environment.NewLine}ExecuteSQL: Exception");
            }
            return intRetCode;
        }

        public ExecuteSQLResult ExecuteSQL2(string sqlScript)
        {
            try
            {
                if (_connection is null)
                {
                    _connection = GetDbConnection();
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                using (var command = GetDbCommand(sqlScript, _connection, _transaction))
                {
                    int executeCount = command.ExecuteNonQuery();
                    if (executeCount <= 0)
                    {
                        WriteSqlScript($"ExecuteSQL: {sqlScript}{Environment.NewLine}ExecuteResult: Success, AffectedRowCount: {executeCount}");
                        return ExecuteSQLResult.SetSuccess(sqlScript, executeCount);
                    }
                    else
                    {
                        WriteSqlScript($"ExecuteSQL: {sqlScript}{Environment.NewLine}ExecuteResult: NoDataUpdate");
                        return ExecuteSQLResult.SetNoDataUpdate(sqlScript);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExceptionLog(ex, $"ExecuteSQL2: {sqlScript}");
                WriteSqlScript($"ExecuteSQL2: {sqlScript}{Environment.NewLine}ExecuteSQL2: Exception");
                return ExecuteSQLResult.SetException(sqlScript, ex);
            }
        }
        public int TransactionCtrl(TransactionTypes transactionType)
        {
            string errorMsg = string.Empty;
            return TransactionCtrl(transactionType, ref errorMsg);
        }
        public int TransactionCtrl(TransactionTypes transactionType, ref string errorMsg)
        {
            int intRetCode = DBResult.Initial;

            try
            {
                switch (transactionType)
                {
                    case TransactionTypes.Begin:
                        if (_transaction == null)
                        {
                            _transaction = _connection.BeginTransaction();
                            intRetCode = DBResult.Success;
                        }
                        else
                        {
                            errorMsg = "Initial Fail";
                            intRetCode = DBResult.Failed;
                        }
                        break;
                    case TransactionTypes.Commit:
                        if (_transaction != null)
                        {
                            _transaction.Commit();
                            _transaction.Dispose();
                            _transaction = null;
                            intRetCode = DBResult.Success;
                            errorMsg = string.Empty;
                        }
                        else
                        {
                            errorMsg = "Initial Fail";
                            intRetCode = DBResult.Failed;
                        }
                        break;
                    case TransactionTypes.Rollback:
                        if (_transaction != null)
                        {
                            _transaction.Rollback();
                            _transaction.Dispose();
                            _transaction = null;
                            intRetCode = DBResult.Success;
                            errorMsg = string.Empty;
                        }
                        else
                        {
                            errorMsg = "Initial Fail";
                            intRetCode = DBResult.Failed;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                intRetCode = DBResult.Exception;
                WriteExceptionLog(ex, $"TransactionCtrl: {nameof(transactionType)}");
                WriteSqlScript($"TransactionCtrl: {GetConnectionString()}{Environment.NewLine}TransactionCtrl: {nameof(transactionType)}");
            }
            return intRetCode;
        }

        public TransactionCtrlResult TransactionCtrl2(TransactionTypes transactionType)
        {
            try
            {
                if (transactionType == TransactionTypes.Begin)
                {
                    if (_transaction == null)
                    {
                        _transaction = _connection.BeginTransaction();
                        return TransactionCtrlResult.SetBeginSuccess();
                    }
                    else
                    {
                        return TransactionCtrlResult.SetBeginFail();
                    }
                }
                else if (transactionType == TransactionTypes.Commit)
                {
                    if (_transaction != null)
                    {
                        _transaction.Commit();
                        _transaction.Dispose();
                        _transaction = null;
                        return TransactionCtrlResult.SetCommitSuccess();
                    }
                    else
                    {
                        return TransactionCtrlResult.SetCommitFail();
                    }
                }
                else
                {
                    if (_transaction != null)
                    {
                        _transaction.Rollback();
                        _transaction.Dispose();
                        _transaction = null;
                        return TransactionCtrlResult.SetRollbackSuccess();
                    }
                    else
                    {
                        return TransactionCtrlResult.SetRollbackFail();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExceptionLog(ex, $"TransactionCtrl: {nameof(transactionType)}");
                WriteSqlScript($"TransactionCtrl: {GetConnectionString()}{Environment.NewLine}TransactionCtrl: {nameof(transactionType)}");
                return TransactionCtrlResult.SetException(transactionType, ex);
            }
        }

        [Obsolete]
        public int GetDataTable(string SQL, ref DataTable dataBase)
        {
            string errorMsg = string.Empty;
            return GetDataTable(SQL, ref dataBase, ref errorMsg);
        }
        [Obsolete]
        public virtual int GetDataTable(string sqlScript, ref DataTable dataBase, ref string errorMsg)
        {
            int intRetCode = DBResult.Initial;
            errorMsg = "Initial Fail";

            dataBase = dataBase ?? new DataTable();

            try
            {
                using (var connection = GetDbConnection())
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        var dataAdapter = GetDbDataAdapter();
                        dataAdapter.SelectCommand = GetDbCommand(sqlScript, connection, null);
                        var dtDataSet = new DataSet();
                        int unused = dataAdapter.Fill(dtDataSet);
                        dataBase = dtDataSet.Tables[0];
                        if (dataBase.Rows.Count > 0)
                        {
                            intRetCode = DBResult.Success;
                            errorMsg = string.Empty;
                        }
                        else
                        {
                            intRetCode = DBResult.NoDataSelect;
                            errorMsg = "No Data Selected";
                        }
                    }
                    else
                    {
                        intRetCode = DBResult.Failed;
                        errorMsg = "Initial Fail";
                    }
                }
            }
            catch (Exception ex)
            {
                intRetCode = DBResult.Exception;
                errorMsg = $"{ex.Message}\n{ex.StackTrace}";
                WriteExceptionLog(ex, $"GetDataTable: {sqlScript}");
                WriteSqlScript($"GetDataTable: {sqlScript}{Environment.NewLine}GetDataTable: Exception");
            }
            return intRetCode;
        }

        public virtual GetDataResult GetData<T>(string sqlScript, out DataObject<T> dataObject) where T : ValueObject, new()
        {
            var dtDataSet = new DataSet();
            try
            {
                using (var connection = GetDbConnection())
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        var dataAdapter = GetDbDataAdapter();
                        dataAdapter.SelectCommand = GetDbCommand(sqlScript, connection, null);
                        int unused = dataAdapter.Fill(dtDataSet);
                        if (dtDataSet.Tables[0].Rows.Count > 0)
                        {
                            dataObject = new DataObject<T>(dtDataSet.Tables[0]);
                            return GetDataResult.SetSuccess(sqlScript);
                        }
                        else
                        {
                            dataObject = new DataObject<T>();
                            return GetDataResult.SetNoDataSelect(sqlScript);
                        }
                    }
                    else
                    {
                        dataObject = new DataObject<T>();
                        return GetDataResult.SetInitial();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExceptionLog(ex, $"GetData: {sqlScript}");
                WriteSqlScript($"GetData: {sqlScript}{Environment.NewLine}GetData: Exception");
                dataObject = new DataObject<T>();
                return GetDataResult.SetException(sqlScript, ex);
            }
            finally
            {
                dtDataSet.Clear();
                dtDataSet.Dispose();
            }
        }

        public bool IsConnected { get; }

        protected void WriteExceptionLog(Exception ex)
        {
            Debug.WriteLine($"{ex}");
            _log.WriteLogFile($"DataBase_{Options.DBName}_Exception", $"\r\n{ex}");
        }

        protected void WriteExceptionLog(Exception ex, string message)
        {
            Debug.WriteLine($"{ex}");
            _log.WriteLogFile($"DataBase_{Options.DBName}_Exception", $"\r\n{ex}\r\n{message}");
        }

        protected void WriteSqlScript(string sqlScript)
        {
            if (Options.WriteLog)
            {
                _log.WriteLogFile($"DataBase_{Options.DBName}_SqlScript", $"{sqlScript}");
            }
        }

        #region Dispose
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _log.Dispose();

                disposedValue = true;
            }
        }

        ~DB()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
