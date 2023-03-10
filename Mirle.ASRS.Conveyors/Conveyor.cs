using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Mirle.ASRS.Conveyors.Signal;
using Mirle.MPLC;
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors
{
    public class Conveyor : IDisposable
    {
        public readonly bool[] _alarmBit = new bool[16];
        private readonly SignalMapper _signal;
        private readonly Dictionary<int, Buffer> _buffers = new Dictionary<int, Buffer>();
        private readonly SystemSignal _systemSignal;
        private readonly ThreadWorker _heartbeat;
        private readonly ThreadWorker _refresh;

        private IMPLCProvider _plcHost;
        public bool IsConnected => _plcHost.IsConnected;

        public delegate void SystemAlarmEventHandler(object sender, AlarmEventArgs e);

        public event SystemAlarmEventHandler OnSystemAlarmTrigger;
        public event SystemAlarmEventHandler OnSystemAlarmClear;

        public IEnumerable<Buffer> Buffers => _buffers.Values;

        public Conveyor(IMPLCProvider plcHost)
        {
            _plcHost = plcHost;
            _signal = new SignalMapper(_plcHost);

            foreach (var buffer in _signal.BufferSignals)
            {
                _buffers.Add(buffer.BufferIndex, new Buffer(buffer));
            }
            _systemSignal = _signal.GetSystemSignal();

            _heartbeat = new ThreadWorker(Heartbeat, 500);
            _refresh = new ThreadWorker(Refresh, 200);
        }

        public void Start()
        {
            _heartbeat.Start();
            _refresh.Start();
        }

        public void Pause()
        {
            _heartbeat.Pause();
            _refresh.Pause();
        }

        public Buffer GetBuffer(int bufferIndex)
        {
            _buffers.TryGetValue(bufferIndex, out var buffer);
            return buffer;
        }
        public bool TryGetBuffer(int bufferIndex, out Buffer buffer)
        {
            return _buffers.TryGetValue(bufferIndex, out buffer);
        }

        private void Heartbeat()
        {
            if (!IsConnected) return;
            var systemSignal = _signal.GetSystemSignal();
            if (systemSignal.Heartbeat.GetValue() == systemSignal.ControllerSignal.Heartbeat.GetValue())
            {
                if (systemSignal.Heartbeat.GetValue() == 0)
                {
                    systemSignal.ControllerSignal.Heartbeat.SetValue(1);
                }
                else
                {
                    systemSignal.ControllerSignal.Heartbeat.SetValue(0);
                }
            }

            var dt = DateTime.Now; 
            int[] bcdDatetime = new int[6];
            bcdDatetime[0] = dt.Year.ConvertBase10ToBCD();
            bcdDatetime[1] = dt.Month.ConvertBase10ToBCD();
            bcdDatetime[2] = dt.Day.ConvertBase10ToBCD();
            bcdDatetime[3] = dt.Hour.ConvertBase10ToBCD();
            bcdDatetime[4] = dt.Minute.ConvertBase10ToBCD();
            bcdDatetime[5] = dt.Second.ConvertBase10ToBCD();

            systemSignal.ControllerSignal.SystemTimeCalibration.SetValue(bcdDatetime);//給予PLC時間調整
        }

        private void Refresh()
        {
            if (!IsConnected) return;
            foreach (var buffer in _buffers.Values)
            {
                buffer.Refresh();
            }
            for (int i = 0; i < 15; i++)
            {
                CheckAlarmBit(i, ref _alarmBit[i]);
            }
        }

        private void CheckAlarmBit(int bitIndex, ref bool reportedFlag)
        {
            if (_systemSignal.Alarm.GetBit(bitIndex).IsOn() && reportedFlag == false)
            {
                reportedFlag = true;
                OnSystemAlarmTrigger?.Invoke(this, new AlarmEventArgs(0, "System", bitIndex));
            }
            else if (_systemSignal.Alarm.GetBit(bitIndex).IsOff() && reportedFlag)
            {
                reportedFlag = false;
                OnSystemAlarmClear?.Invoke(this, new AlarmEventArgs(0, "System", bitIndex));
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
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        ~Conveyor()
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
