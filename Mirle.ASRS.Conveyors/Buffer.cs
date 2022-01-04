﻿using System;
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

        public delegate void AlarmEventHandler(object sender, AlarmEventArgs e);
        public delegate void BufferEventHandler(object sender, BufferEventArgs e);
        public delegate void InitialNoticeEventHandler(object sender, BufferEventArgs e);

        public event AlarmEventHandler OnAlarmTrigger;
        public event AlarmEventHandler OnAlarmClear;
        public event BufferEventHandler OnBufferCommandReceive;
        public event BufferEventHandler OnBufferPathNoticeChange;
        public event InitialNoticeEventHandler OnIniatlNotice;
        public event InitialNoticeEventHandler OnIniatlNoticeComplete;

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
        public int PickingDirection => Signal.PickingDirection.GetValue();
        public int TrayType => Signal.TrayType.GetValue();
        public int Switch_Ack => Signal.Switch_Ack.GetValue();
        public int EmptyINReady => Signal.EmptyInReady.GetValue();
        public int A2LV2 => Signal.A2LV2.GetValue();


        public Buffer(BufferSignal signal)
        {
            Signal = signal;
        }

        protected internal virtual void Refresh()
        {
            if (Signal.CmdMode.GetValue() > 0 && Signal.CmdMode.GetValue() == Signal.ControllerSignal.LoadCategory.GetValue()
                )
            {
                Signal.ControllerSignal.LoadCategory.Clear();
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
            else if (Signal.CommandId.GetValue() > 0 && Signal.CommandId.GetValue() == Signal.ControllerSignal.CommandId.GetValue())
            {
                Signal.ControllerSignal.CommandId.Clear();
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }

            if (Signal.ControllerSignal.PathChangeNotice.GetValue() > 0)
            {
                Signal.ControllerSignal.PathChangeNotice.SetValue(0);
                OnBufferPathNoticeChange?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }

            if (Signal.ControllerSignal.A4Emptysupply.GetValue() > 0)
            {
                Signal.ControllerSignal.A4Emptysupply.SetValue(0);
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }

            if (Signal.ControllerSignal.Switch_Mode.GetValue() > 0)
            {
                Signal.ControllerSignal.Switch_Mode.SetValue(0);
                OnBufferCommandReceive?.Invoke(this, new BufferEventArgs(Signal.BufferIndex, Signal.BufferName));
            }
            CheckIniatlNotice();
            CheckAlarmBit();
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

        public Task<bool> WriteCommandIdAsync(string commandId, int loadCategory)
        {
            return Task.Run(() =>
            {
                int[] value = new int[2];
                value[1] = loadCategory;
            int.TryParse(commandId, out value[0]);
                if(Signal.ControllerSignal.CommandId.SetValue(value[0])==true)
                 {
                    return Signal.ControllerSignal.LoadCategory.SetValue(value[1]);
                }
                else
                {
                    return Signal.ControllerSignal.CommandId.SetValue(value[0]);
                }

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

        public Task<bool> WritePathChabgeNotice(int path)
        {
            return Task.Run(() =>
            {
                return Signal.ControllerSignal.PathChangeNotice.SetValue(path);
            });
        }
    }
}
