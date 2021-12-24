using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Mirle.Logger
{
    public class AutoArchive : IDisposable
    {
        private readonly Timer _autoArchiveProcess = new Timer();
        private readonly Timer _deleteProcess = new Timer();

        public AutoArchiveOptions Options { get; }

        [Obsolete] public int AutoArchiveFileDay => Options.ArchiveDay;
        [Obsolete] public int AutoDeleteArchiveFileDay => Options.DeleteDay;

        public AutoArchive() : this(new AutoArchiveOptions())
        {
        }
        public AutoArchive(int compressDay) : this(new AutoArchiveOptions(compressDay))
        {
        }
        public AutoArchive(int compressDay, int deleteDay) : this(new AutoArchiveOptions(compressDay, deleteDay))
        {
        }
        public AutoArchive(AutoArchiveOptions options)
        {
            Options = options;

            _autoArchiveProcess.Elapsed += new ElapsedEventHandler(AutoArchiveProcess_Elapsed);
            _autoArchiveProcess.Interval = 30_000;
            _deleteProcess.Elapsed += new ElapsedEventHandler(DeleteProcess_Elapsed);
            _deleteProcess.Interval = 15_000;
        }

        public void Start()
        {
            _autoArchiveProcess.Start();
            _deleteProcess.Start();
        }
        public void Stop()
        {
            _autoArchiveProcess.Stop();
            _deleteProcess.Stop();
        }

        private void AutoArchiveProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            _autoArchiveProcess.Stop();
            if (Options.ArchiveDay > 0)
            {
                AutoArchiveProcess();
            }
            var nextExecute = DateTime.Today.AddDays(1) - DateTime.Now;
            _autoArchiveProcess.Interval = nextExecute.TotalMilliseconds + 30_000;
            _autoArchiveProcess.Start();
        }

        private void DeleteProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            _deleteProcess.Stop();
            if (Options.ArchiveDay > 0 && Options.DeleteDay > 0)
            {
                DeleteArchiveProcess();
            }
            else if (Options.DeleteDay > 0)
            {
                DeleteLogProcess();
            }
            var nextExecute = DateTime.Today.AddDays(1) - DateTime.Now;
            _deleteProcess.Interval = nextExecute.TotalMilliseconds + 15_000;
            _deleteProcess.Start();
        }

        private void AutoArchiveProcess()
        {
            var dirLogPath = new DirectoryInfo(Options.Directory);
            foreach (var directoryInfo in dirLogPath.GetFileSystemInfos())
            {
                try
                {
                    SpinWait.SpinUntil(() => false, 10);
                    string zipName = $@"{Options.ArchiveDirectory}\{directoryInfo.Name}.zip";
                    if (DateTime.TryParse(directoryInfo.Name, out var dateTime))
                    {
                        if (dateTime.AddDays(Options.ArchiveDay) < DateTime.Now)
                        {
                            if (File.Exists(zipName))
                            {
                                string destFileName = $@"{Options.ArchiveDirectory}\{directoryInfo.Name}_{DateTime.Now:yyyyMMddHHmmss}_Backup.zip";
                                File.Copy(zipName, destFileName);
                                File.Delete(zipName);
                            }

                            ZipFile.CreateFromDirectory(directoryInfo.FullName, zipName);
                            if (File.Exists(zipName))
                            {
                                directoryInfo.Delete();
                            }
                        }
                    }
                    else if (directoryInfo.CreationTime.AddDays(Options.ArchiveDay) < DateTime.Now)
                    {
                        if (File.Exists(zipName))
                        {
                            string destFileName = $@"{Options.ArchiveDirectory}\{directoryInfo.Name}_{DateTime.Now:yyyyMMddHHmmss}_Backup.zip";
                            File.Copy(zipName, destFileName);
                            File.Delete(zipName);
                        }

                        ZipFile.CreateFromDirectory(directoryInfo.FullName, zipName);
                        if (File.Exists(zipName))
                        {
                            directoryInfo.Delete();
                        }
                    }
                }
                catch (Exception ex)
                { Debug.WriteLine($"{ex}"); }
            }
        }

        private void DeleteArchiveProcess()
        {
            try
            {
                var dirLogPath = new DirectoryInfo(Options.Directory);
                foreach (var fileInfo in dirLogPath.GetFiles("*.tmp"))
                {
                    if (fileInfo.CreationTime.AddMinutes(360) < DateTime.Now)
                    {
                        fileInfo.Delete();
                    }
                }

                foreach (var fileInfo in dirLogPath.GetFiles("*.zip"))
                {
                    var objDateTime = DateTime.Now;
                    if (DateTime.TryParse(fileInfo.Name.Replace("*.zip", string.Empty), out objDateTime))
                    {
                        if (objDateTime.AddDays(Options.DeleteDay) < DateTime.Now)
                        {
                            fileInfo.Delete();
                        }
                    }
                    else if (fileInfo.CreationTime.AddDays(Options.DeleteDay) < DateTime.Now)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch (Exception ex)
            { Debug.WriteLine($"{ex}"); }
        }

        private void DeleteLogProcess()
        {
            var dirLogPath = new DirectoryInfo(Options.Directory);
            foreach (var directoryInfo in dirLogPath.GetFileSystemInfos())
            {
                try
                {
                    SpinWait.SpinUntil(() => false, 10);
                    directoryInfo.Delete();
                }
                catch (Exception ex)
                { Debug.WriteLine($"{ex}"); }
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
                    Stop();

                    _autoArchiveProcess.Dispose();
                    _deleteProcess.Dispose();
                }

                disposedValue = true;
            }
        }

        ~AutoArchive()
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
