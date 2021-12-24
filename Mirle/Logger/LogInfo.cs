using System;

namespace Mirle.Logger
{
    public sealed class LogInfo : IEquatable<LogInfo>
    {
        public DateTime CreateTime { get; }
        public string Subdirectory { get; }
        public string FileName { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        private LogInfo() : this(string.Empty, string.Empty, LogLevel.Trace, string.Empty)
        {
        }
        public LogInfo(string fileName, string message) : this(string.Empty, fileName, LogLevel.Trace, message)
        {
        }
        public LogInfo(string fileName, LogLevel level, string message) : this(string.Empty, fileName, level, message)
        {
        }
        public LogInfo(string subdirectory, string fileName, string message) : this (subdirectory, fileName, LogLevel.Trace, message)
        {
        }
        public LogInfo(string subdirectory, string fileName, LogLevel level, string message)
        {
            CreateTime = DateTime.Now;
            Subdirectory = subdirectory;
            if (string.IsNullOrWhiteSpace(subdirectory))
            {
                if (fileName.Contains(".log"))
                {
                    FileName = fileName;
                }
                else if (string.IsNullOrWhiteSpace(fileName) == false)
                {
                    FileName = $@"{fileName}.log";
                }
                else
                {
                    FileName = $@"{DateTime.Now:yyyyMMdd}.log";
                }
            }
            else
            {
                if (fileName.Contains(".log"))
                {
                    FileName = $@"{subdirectory}\{fileName}";
                }
                else if (string.IsNullOrWhiteSpace(fileName) == false)
                {
                    FileName = $@"{subdirectory}\{fileName}.log";
                }
                else
                {
                    FileName = $@"{subdirectory}\{DateTime.Now:yyyyMMdd}.log";
                }
            }
            Level = level;
            Message = message;
        }

        public bool Equals(LogInfo other)
        {
            return CreateTime == other?.CreateTime;
        }

        public override bool Equals(object obj)
        {
            if (obj is LogInfo)
            {
                return Equals(obj as LogInfo);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return 0;
        }
        public override string ToString()
        {
            if (Level == LogLevel.Empty)
            {
                return $"{CreateTime:[HH:mm:ss.fff]} {Message}";
            }
            else
            {
                return $"{CreateTime:[HH:mm:ss.fff]} [{Level}] {Message}";
            }
        }

        #region operator
        public static bool operator ==(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return true;
            else
                return (logInfo1 ?? new LogInfo()).Equals(logInfo2);
        }
        public static bool operator !=(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return false;
            else
                return (logInfo1 ?? new LogInfo()).Equals(logInfo2);
        }
        public static bool operator >(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return false;
            else
                return (logInfo1 ?? new LogInfo()).CreateTime > logInfo2.CreateTime;
        }
        public static bool operator >=(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return true;
            else
                return (logInfo1 ?? new LogInfo()).CreateTime >= logInfo2.CreateTime;
        }
        public static bool operator <(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return false;
            else
                return (logInfo1 ?? new LogInfo()).CreateTime < logInfo2.CreateTime;
        }
        public static bool operator <=(LogInfo logInfo1, LogInfo logInfo2)
        {
            if (ReferenceEquals(logInfo1, logInfo2))
                return true;
            else
                return (logInfo1 ?? new LogInfo()).CreateTime <= logInfo2.CreateTime;
        }
        #endregion operator
    }
}
