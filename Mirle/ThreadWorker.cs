using System;
using System.Diagnostics;
using System.Threading;

namespace Mirle
{
    public sealed class ThreadWorker : IDisposable
    {
        private readonly Action _task;

        private int _interval = 1000;
        private bool _runFlag = true;
        private DateTime _nextCanRunTime = DateTime.Now;

        public int Interval
        {
            get => _interval;
            set => _interval = value < 100 ? 100 : value;
        }

        public bool IsRunning { get; private set; } = false;

        public ThreadWorker(Action task) : this(task, 100)
        {
        }
        public ThreadWorker(Action task, int interval)
        {
            _task = task;
            _interval = interval;

            var thr = new Thread(new ThreadStart(WorkProcess));
            thr.IsBackground = true;
            thr.Start();
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Start()
        {
            IsRunning = true;
        }

        private void WorkProcess()
        {
            while (_runFlag)
            {
                if (IsRunning && DateTime.Now > _nextCanRunTime)
                {
                    try
                    {
                        _nextCanRunTime = DateTime.Now.AddMilliseconds(_interval);
                        _task?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex}");
                    }
                }
                else if (_nextCanRunTime > DateTime.Now.AddMilliseconds(_interval))
                {
                    _nextCanRunTime = DateTime.Now;
                    continue;
                }

                SpinWait.SpinUntil(() => false, 10);
            }
        }

        #region Dispose
        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                _runFlag = false;

                disposedValue = true;
            }
        }

        ~ThreadWorker()
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