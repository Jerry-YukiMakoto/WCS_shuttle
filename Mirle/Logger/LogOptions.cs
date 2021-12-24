using System;
using System.Collections.Generic;
using System.IO;

namespace Mirle.Logger
{
    public sealed class LogOptions
    {
        private readonly DirectoryInfo _logDirectory = new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log");

        public string LogDirectory => _logDirectory.FullName;

        public LogOptions() : this(new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log"))
        {
        }
        public LogOptions(string path) : this(new DirectoryInfo($@"{path}"))
        {
        }
        public LogOptions(DirectoryInfo logDirectory)
        {
            _logDirectory = logDirectory;
            _logDirectory.Create();
        }

        public override string ToString()
        {
            return _logDirectory.FullName;
        }
    }
}
