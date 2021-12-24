using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Logger
{
    public class Log : IDisposable
    {
        private readonly ConcurrentQueue<LogInfo> _logQueue = new ConcurrentQueue<LogInfo>();

        private bool _runFlag = true;

        public LogOptions Options { get; }

        public Log() : this(new LogOptions())
        {
        }
        public Log(LogOptions options)
        {
            Options = options;
            Task.Run(() => WriteLog());
        }

        public void WriteLogFile(string logFileName, string logString)
        {
            WriteLogFile(new LogInfo(logFileName, logString));
        }
        public void WriteLogFile(LogInfo logInfo)
        {
            _logQueue.Enqueue(logInfo);
        }

        private void WriteLog()
        {
            while (_runFlag)
            {
                try
                {
                    while (_logQueue.TryPeek(out var logItem))
                    {
                        if (Directory.Exists(Options.LogDirectory) == false)
                        {
                            Directory.CreateDirectory(Options.LogDirectory);
                        }

                        if (Directory.Exists($@"{Options.LogDirectory}\{DateTime.Now:yyyy-MM-dd}") == false)
                        {
                            Directory.CreateDirectory($@"{Options.LogDirectory}\{DateTime.Now:yyyy-MM-dd}");
                        }

                        if (string.IsNullOrWhiteSpace(logItem.Subdirectory) == false)
                        {
                            Directory.CreateDirectory($@"{Options.LogDirectory}\{DateTime.Now:yyyy-MM-dd}\{logItem.Subdirectory}");
                        }

                        using (var fileStream = new FileStream($@"{Options.LogDirectory}\{DateTime.Now:yyyy-MM-dd}\{logItem.FileName}", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                streamWriter.WriteLine(logItem.ToString());
                                _logQueue.TryDequeue(out logItem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }
                SpinWait.SpinUntil(() => false, 500);
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
                }

                disposedValue = true;
                _runFlag = false;
            }
        }

        ~Log()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}
