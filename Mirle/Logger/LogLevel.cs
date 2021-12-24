using System;

namespace Mirle.Logger
{
    public sealed class LogLevel : IComparable, IComparable<LogLevel>, IEquatable<LogLevel>
    {
        public static readonly LogLevel Empty = new LogLevel("Empty", 0);
        public static readonly LogLevel Trace = new LogLevel("Trace", 1);
        public static readonly LogLevel Warning = new LogLevel("Warning", 2);
        public static readonly LogLevel Error = new LogLevel("Error", 3);
        public static readonly LogLevel Debug = new LogLevel("Debug", 4);

        public string LevelName { get; }
        public int Ordinal { get; }

        private LogLevel(string levelName, int ordinal)
        {
            LevelName = levelName;
            Ordinal = ordinal;
        }

        public int CompareTo(LogLevel other)
        {
            return Ordinal - (other ?? Empty).Ordinal;
        }
        public int CompareTo(object obj)
        {
            return CompareTo((LogLevel)obj);
        }
        public bool Equals(LogLevel other)
        {
            return Ordinal == other?.Ordinal;
        }

        public override bool Equals(object obj)
        {
            if (obj is LogLevel)
            {
                return Equals(obj as LogLevel);
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return LevelName;
        }
        public override int GetHashCode()
        {
            return Ordinal;
        }

        #region operator
        public static bool operator ==(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? Empty).Equals(level2);
        }
        public static bool operator !=(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return !(level1 ?? Empty).Equals(level2);
        }
        public static bool operator >(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return (level1 ?? Empty).CompareTo(level2) > 0;
        }
        public static bool operator >=(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? Empty).CompareTo(level2) >= 0;
        }
        public static bool operator <(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return (level1 ?? Empty).CompareTo(level2) < 0;
        }
        public static bool operator <=(LogLevel level1, LogLevel level2)
        {
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? Empty).CompareTo(level2) <= 0;
        }
        #endregion operator
    }
}
