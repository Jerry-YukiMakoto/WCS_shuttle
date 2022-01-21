using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Mirle.Logger;

namespace Mirle.ASRS.Conveyor.U2NMMA30.Service
{
    public class LoggerService : IDisposable
    {
        private readonly string _ConveyorId;
        private readonly Log _Log = new Log();
        private readonly object _Lock = new object();

        public LoggerService(string conveyorId)
        {
            _ConveyorId = conveyorId;
        }

        public void WriteExceptionLog(MethodBase methodBase, string message)
        {
            try
            {
                lock (_Lock)
                {
                    Debug.WriteLine($"{methodBase.DeclaringType.FullName}.{methodBase.Name}: {message}");
                    _Log.WriteLogFile($"{_ConveyorId}_Exception.log", $"{methodBase.DeclaringType.FullName}.{methodBase.Name}: {message}");
                }
            }
            catch (Exception ex)
            { Debug.WriteLine(ex.Message); }
        }

        public void WriteLog(string msg)
        {
            try
            {
                lock (_Lock)
                {
                    _Log.WriteLogFile($"{_ConveyorId}_SysTrace", msg);
                }
            }
            catch (Exception ex)
            { WriteExceptionLog(MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}"); }
        }

        public void WriteLog(string fileName, string msg)
        {
            try
            {
                lock (_Lock)
                {
                    _Log.WriteLogFile(fileName, msg);
                }
            }
            catch (Exception ex)
            { WriteExceptionLog(MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}"); }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                _Log?.Dispose();

                disposedValue = true;
            }
        }

        ~LoggerService()
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
