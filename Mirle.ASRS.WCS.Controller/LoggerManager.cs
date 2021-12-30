using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.Logger;

namespace Mirle.ASRS.WCS.Controller
{
    public class LoggerManager : IDisposable
    {
        private readonly Log _log = new Log();
        private readonly AutoArchive autoArchive = new AutoArchive();
        private readonly ConcurrentQueue<string> _traceQueue = new ConcurrentQueue<string>();
        private readonly object _logLock = new object();

        public LoggerManager()
        {
            Task.Delay(30_000).ContinueWith(r =>
            {
                autoArchive.Start();
            });
        }

        public void WriteLogTrace(LogTraceBase logTraceBase)
        {
            try
            {
                lock (_logLock)
                {
                    _traceQueue.Enqueue($"{DateTime.Now:[HH:mm:ss.fff]} {logTraceBase.GetMessage()}");
                    _log.WriteLogFile($"WCS_Trace.log", logTraceBase.GetMessage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        public void Error(Exception exception)
        {
            try
            {
                lock (_logLock)
                {
                    _log.WriteLogFile($"WCS_Exception.log", $"{exception}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        public IEnumerable<string> GetCurrentTrace()
        {
            while (_traceQueue.Any())
            {
                _traceQueue.TryDequeue(out var result);
                yield return result;
            }
        }

        #region Dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _log.Dispose();
                    autoArchive.Dispose();
                }

                disposedValue = true;
            }
        }

        ~LoggerManager()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}
