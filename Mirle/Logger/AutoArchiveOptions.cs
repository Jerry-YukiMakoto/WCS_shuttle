using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirle.Logger
{
    public sealed class AutoArchiveOptions
    {
        private readonly DirectoryInfo _directory = new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log");
        private readonly DirectoryInfo _archiveDirectory = new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Archive");

        public int ArchiveDay { get; } = 1;
        public int DeleteDay { get; } = 90;
        public string Directory => _directory.FullName;
        public string ArchiveDirectory => _archiveDirectory.FullName;

        public AutoArchiveOptions() : this(1, 90, new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log"), new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Archive"))
        {
        }
        public AutoArchiveOptions(int archiveDay) : this(archiveDay, 90, new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log"), new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Archive"))
        {
        }
        public AutoArchiveOptions(int archiveDay, int deleteDay) : this(archiveDay, deleteDay, new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Log"), new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Archive"))
        {
        }
        public AutoArchiveOptions(int archiveDay, int deleteDay, DirectoryInfo directory) : this(archiveDay, deleteDay, directory, new DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}Archive"))
        {
        }
        public AutoArchiveOptions(int archiveDay, int deleteDay, DirectoryInfo directory, DirectoryInfo archiveDirectory)
        {
            ArchiveDay = archiveDay;
            DeleteDay = deleteDay;

            _directory = directory;
            _archiveDirectory = archiveDirectory;

            _directory.Create();
            _archiveDirectory.Create();
        }

        public override string ToString()
        {
            return ArchiveDirectory;
        }
    }
}
