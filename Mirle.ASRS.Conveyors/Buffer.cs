using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.ASRS.Conveyors.Signal;
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors
{
    public class Buffer
    {
        private readonly bool[] _alarmBit = new bool[16];

        private bool _onIniatlNotice = false;
        private bool _onAutomaticDoor = false;
        private bool _onReadNotice = false;

        public delegate void AlarmEventHandler(object sender, AlarmEventArgs e);
        public delegate void BufferEventHandler(object sender, BufferEventArgs e);
        public delegate void InitialNoticeEventHandler(object sender, BufferEventArgs e);
        public delegate void AutomaticDoorEventHandler(object sender, BufferEventArgs e);
        public delegate void ReadNoticeEventHandler(object sender, ReadNoticeEventArgs e);

        public event AlarmEventHandler OnAlarmTrigger;
        public event AlarmEventHandler OnAlarmClear;
        public event BufferEventHandler OnBufferCommandReceive;
        public event BufferEventHandler OnBufferPathNoticeChange;
        public event InitialNoticeEventHandler OnIniatlNotice;
        public event InitialNoticeEventHandler OnIniatlNoticeComplete;
        public event AutomaticDoorEventHandler OnAutomaticDoorOpend;
        public event AutomaticDoorEventHandler OnAutomaticDoorClose;
        public event ReadNoticeEventHandler OnReadNotice;

        public BufferSignal Signal { get; }

        public int BufferIndex => Signal.BufferIndex;
        public string BufferName => Signal.BufferName;

        public int CommandId => Signal.CommandId.GetValue();
        public int PathNotice => Signal.PathNotice.GetValue();
        public int LoadCategory => Signal.LoadCategory.GetValue();
        public int CmdMode => Signal.CmdMode.GetValue();
        public int Ready => Signal.Ready.GetValue();
        public int ReadNotice => Signal.ReadNotice.GetValue();
        public int InitialNotice => Signal.InitialNotice.GetValue();
        public bool InMode => Signal.StatusSignal.InMode.IsOn() && Signal.StatusSignal.OutMode.IsOff();
        public bool OutMode => Signal.StatusSignal.InMode.IsOff() && Signal.StatusSignal.OutMode.IsOn();
        public bool Error => Signal.StatusSignal.Error.IsOn();
        public bool Auto => Signal.StatusSignal.Auto.IsOn() && Signal.StatusSignal.Manual.IsOff();
        public bool Manual => Signal.StatusSignal.Auto.IsOff() && Signal.StatusSignal.Manual.IsOn();
        public bool Presence => Signal.StatusSignal.Presence.IsOn();
        public bool Position => Signal.StatusSignal.Position.IsOn();
        public bool Finish => Signal.StatusSignal.Finish.IsOn();
        public int PickingDirection => Signal.PickingDirection.GetValue();
        public int TrayType => Signal.TrayType.GetValue();
        public bool AutomaticDoor => Signal.StatusSignal.AutomaticDoor.IsOn();
        public bool LoadReq => Signal.StatusSignal.LoadReq.IsOn();
        public bool LoadFinish => Signal.StatusSignal.LoadFinish.IsOn();
        public bool UnloadReq => Signal.StatusSignal.UnloadReq.IsOn();
        public bool UnloadFinish => Signal.StatusSignal.UnloadFinish.IsOn();

        public int Switch_Ack => Signal.Switch_Ack.GetValue();

        public int EmptyINReady => Signal.EmptyInReady.GetValue();

        public int A2LV2 => Signal.A2LV2.GetValue();

        public int Weight => Signal.DataBuffer.Weight.GetValue();
        public string TrayId => Signal.DataBuffer.TrayId.GetValue().ToASCII();
        public string FosbId => Signal.DataBuffer.FosbId.GetValue().ToASCII();
        public string Plant => Signal.DataBuffer.Plant.GetValue().ToASCII();
        public string Plant_Left => Signal.DataBuffer.Plant_Left.GetValue().ToASCII();
        public string Plant_Right => Signal.DataBuffer.Plant_Right.GetValue().ToASCII();
        public string FosbId_Left => Signal.DataBuffer.FosbId_Left.GetValue().ToASCII();
        public string FosbId_Right => Signal.DataBuffer.FosbId_Right.GetValue().ToASCII();

        public Buffer(BufferSignal signal)
        {
            Signal = signal;
        }

        protected internal virtual void Refresh()
        {
            if (Signal.PathNotice.GetValue() > 0 && Signal.PathNotice.GetValue() == Signal.ControllerSignal.PathNotice.GetValue()
                && Signal.LoadCategory.GetValue() > 0 && Signal.LoadCategory.GetValue() == Signal.ControllerSignal.LoadCategory.GetValue()
                && Signal.CommandId.GetValue() > 0 && Signal.CommandId.GetValue() == Signal.ControllerSignal.CommandId.GetValue())
            {
                Signal.ControllerSignal.CommandData.Clear();
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
            else if (Signal.LoadCategory.GetValue() > 0 && Signal.LoadCategory.GetValue() == Signal.ControllerSignal.LoadCategory.GetValue()
                && Signal.CommandId.GetValue() > 0 && Signal.CommandId.GetValue() == Signal.ControllerSignal.CommandId.GetValue())
            {
                Signal.ControllerSignal.CommandData.Clear();
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }

            if (Signal.PathNotice.GetValue() > 0 && Signal.PathNotice.GetValue() == Signal.ControllerSignal.PathNotice.GetValue())
            {
                Signal.ControllerSignal.PathNotice.SetValue(0);
                OnBufferPathNoticeChange?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }

            CheckReadNotice();
            CheckAutomaticDoor();
            CheckIniatlNotice();
            CheckAlarmBit();
        }

        private void CheckReadNotice()
        {
            if (_onReadNotice == false && Signal.ReadNotice.GetValue() == 1)
            {
                _onReadNotice = true;
                int weight = Signal.DataBuffer.Weight.GetValue();
                string trayId = Signal.DataBuffer.TrayId.GetValue().ToASCII();
                string fosbId = Signal.DataBuffer.FosbId.GetValue().ToASCII();
                string fosbId_Left = Signal.DataBuffer.FosbId_Left.GetValue().ToASCII();
                string fosbId_Right = Signal.DataBuffer.FosbId_Right.GetValue().ToASCII();
                string plant = Signal.DataBuffer.Plant.GetValue().ToASCII();
                string plant_Left = Signal.DataBuffer.Plant_Left.GetValue().ToASCII();
                string plant_Right = Signal.DataBuffer.Plant_Right.GetValue().ToASCII();
                OnReadNotice?.Invoke(this, new ReadNoticeEventArgs(Signal.BufferIndex, Signal.BufferName, weight, trayId, fosbId, fosbId_Left, fosbId_Right, plant, plant_Left, plant_Right));
            }
            else if (_onReadNotice == true && Signal.ReadNotice.GetValue() == 0)
            {
                _onReadNotice = false;
            }
        }
        private void CheckAlarmBit()
        {
            for (int i = 0; i < 15; i++)
            {
                CheckAlarmBit(i, ref _alarmBit[i]);
            }
        }
        private void CheckAlarmBit(int bitIndex, ref bool reportedFlag)
        {
            if (Signal.Alarm.GetBit(bitIndex).IsOn() && reportedFlag == false)
            {
                reportedFlag = true;
                OnAlarmTrigger?.Invoke(this, new AlarmEventArgs(Signal.BufferIndex, Signal.BufferName, bitIndex));
            }
            else if (Signal.Alarm.GetBit(bitIndex).IsOff() && reportedFlag)
            {
                reportedFlag = false;
                OnAlarmClear?.Invoke(this, new AlarmEventArgs(Signal.BufferIndex, Signal.BufferName, bitIndex));
            }
        }
        private void CheckIniatlNotice()
        {
            if (_onIniatlNotice == false && Signal.ControllerSignal.InitialNotice.GetValue() == 1)
            {
                _onIniatlNotice = true;
                OnIniatlNotice?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
            else if (_onIniatlNotice == true && Signal.ControllerSignal.InitialNotice.GetValue() == 1 && Signal.InitialNotice.GetValue() == 1)
            {
                _onIniatlNotice = false;
                Signal.ControllerSignal.InitialNotice.SetValue(0);
                OnIniatlNoticeComplete?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
        }
        private void CheckAutomaticDoor()
        {
            if (_onAutomaticDoor == false && Signal.StatusSignal.AutomaticDoor.IsOn())
            {
                _onAutomaticDoor = true;
                OnAutomaticDoorOpend?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
            else if (_onAutomaticDoor == true && Signal.StatusSignal.AutomaticDoor.IsOff())
            {
                _onAutomaticDoor = false;
                OnAutomaticDoorClose?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
        }
        public Task WriteCommandIdAsync(string commandId, int loadCategory)
        {
            return Task.Run(() =>
            {
                int[] value = new int[2];
                value[1] = loadCategory;
                if (int.TryParse(commandId, out value[0]))
                {
                    Signal.ControllerSignal.CommandId.SetValue(value[0]);
                    Signal.ControllerSignal.LoadCategory.SetValue(value[1]);
                }
            });
        }

        public Task WriteCommandIdAsync(string commandId, int path, int loadCategory)
        {
            return Task.Run(() =>
            {
                int[] value = new int[3];
                value[1] = path;
                value[2] = loadCategory;
                if (int.TryParse(commandId, out value[0]))
                {
                    Signal.ControllerSignal.CommandData.SetValue(value);
                }
            });
        }
        public Task WritePathNoticeAsync(int path)
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.PathNotice.SetValue(path);
                Signal.ControllerSignal.PathChangeNotice.SetValue(1);
            });
        }
        public Task WriteNoticeInitalAsync()
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.InitialNotice.SetValue(1);
            });
        }
        public Task AutomaticDoorOpendAsync()
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.AutomaticDoorOpend.SetOn();
            });
        }
        public Task AutomaticDoorClosedAsync()
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.AutomaticDoorClosed.SetOn();
            });
        }
        public Task A4EmptysupplyOn()
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.A4Emptysupply.SetValue(1);
            });
        }
        public Task InitialNoticeTrigger()
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.InitialNotice.SetValue(1);
            });
        }

        public Task Switch_Mode(int mode)
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.Switch_Mode.SetValue(mode);
            });
        }

        public Task WritePathChabgeNotice(int path)
        {
            return Task.Run(() =>
            {
                Signal.ControllerSignal.PathChangeNotice.SetValue(path);
            });
        }
    }
}
