using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.MPLC;
using Mirle.MPLC.DataBlocks;
using Mirle.MPLC.DataBlocks.DeviceRange;
using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.Signal
{
    public class SignalMapper
    {
        private readonly IMPLCProvider _mplc;
        private readonly Dictionary<int, BufferSignal> _bufferSignals = new Dictionary<int, BufferSignal>();
        private readonly SystemSignal _systemSignal = new SystemSignal();
        private readonly int _signalGroup = 0;

        public IEnumerable<BufferSignal> BufferSignals => _bufferSignals.Values;

        public SignalMapper(IMPLCProvider mplc)
        {
            _mplc = mplc;

            MappingSystem();
            MappingBuffer();
        }
        private List<ConveyorDefine> GetDefaultMaps()
        {
            List<ConveyorDefine> Conveyors = new List<ConveyorDefine>();
            ConveyorDefine conveyor = new ConveyorDefine();
            conveyor.SignalGroup = 0;
            conveyor.Buffers = new List<BufferDefine>()
            {
                new BufferDefine() { BufferIndex = 1, BufferName = "A1" },
                new BufferDefine() { BufferIndex = 2, BufferName = "A2" },
                new BufferDefine() { BufferIndex = 3, BufferName = "A3" },
                new BufferDefine() { BufferIndex = 4, BufferName = "A4" },
                new BufferDefine() { BufferIndex = 5, BufferName = "A5" },
                new BufferDefine() { BufferIndex = 6, BufferName = "A6" },
                new BufferDefine() { BufferIndex = 7, BufferName = "A7" },
                new BufferDefine() { BufferIndex = 8, BufferName = "A8" },
                new BufferDefine() { BufferIndex = 9, BufferName = "A9" },
                new BufferDefine() { BufferIndex = 10, BufferName = "A10" },
            };
            Conveyors.Add(conveyor);

            return Conveyors;
        }

        private void MappingSystem()
        {
            int plcIndex = 101;
            int pcIndex = 3101;
            _systemSignal.Heartbeat = new Word(_mplc, $"D{plcIndex}");
            _systemSignal.Alarm = new Word(_mplc, $"D{plcIndex + 6}");

            _systemSignal.ControllerSignal.Heartbeat = new Word(_mplc, $"D{pcIndex}");
            _systemSignal.ControllerSignal.SystemTimeCalibration = new WordBlock(_mplc, $"D{pcIndex + 1}", 6);

        }

        private void MappingBuffer()
        {
            var fileName = @".\Config\SignalDefine.xml";
            List<ConveyorDefine> define = XmlFile.Deserialize<List<ConveyorDefine>>(fileName);
            if (define is null)
            {
                define = GetDefaultMaps();
                define.Serialize(fileName);
            }

                    int plcIndex = 111;
                    int pcIndex = 3111;

                    var conveyor = define.Find(r => r.SignalGroup == _signalGroup);

                    var readyBufferIndex = new Dictionary<int, int>();
                    readyBufferIndex.Add(1, 0);//A1
                    readyBufferIndex.Add(4, 3);//A4
                    readyBufferIndex.Add(5, 4);//A5
                    readyBufferIndex.Add(7, 6);//A7
                    readyBufferIndex.Add(9, 8);//A9

                    var PathChangeNotice = new Dictionary<int, int>();
                    PathChangeNotice.Add(1, 0);//A1
                    PathChangeNotice.Add(2, 1);//A2
                    PathChangeNotice.Add(3, 2);//A3
                    PathChangeNotice.Add(4, 3);//A4

                    for (int bufferIndex = 0; bufferIndex < conveyor.Buffers.Count; bufferIndex++)
                    {
                        string bufferName = string.Empty;
                        if (conveyor.Buffers.Exists(r => r.BufferIndex == bufferIndex + 1))
                        {
                            var bufferDefine = conveyor.Buffers.Find(r => r.BufferIndex == bufferIndex + 1);
                            bufferName = bufferDefine.BufferName;
                        }

                        var buffer = new BufferSignal(bufferIndex + 1, bufferName);
                        buffer.CommandId = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10)}");
                        buffer.CmdMode = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 1}");

                        buffer.StatusSignal.InMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.1");
                        buffer.StatusSignal.OutMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.2");
                        buffer.StatusSignal.Error = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.4");
                        buffer.StatusSignal.Auto = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.5");
                        buffer.StatusSignal.Manual = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.6");
                        buffer.StatusSignal.Presence = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.7");
                        buffer.StatusSignal.Position = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.8");
                        buffer.StatusSignal.Finish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.9");
                        buffer.StatusSignal.EmergencyStop = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}.A");

                        if (readyBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.Ready = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}");
                        }
                        else
                        {
                            buffer.Ready = new Word();
                        }

                        if (PathChangeNotice.ContainsKey(bufferIndex + 1))
                        {
                            buffer.PathChangeNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 4}");
                        }
                        else
                        {
                            buffer.PathChangeNotice = new Word();
                        }

                        buffer.Alarm = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 6}");

                        buffer.InitialNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 9}");

                        #region PLC->PC增加單一特殊點位位置
                        if (bufferIndex + 1 == 1)
                        {
                            buffer.Switch_Ack = new Word(_mplc, $"D119");//只有A1有需要切換通知
                        }
                        else
                        {
                            buffer.Switch_Ack = new Word();
                        }
                        if (bufferIndex + 1 == 2)
                        {
                            buffer.A2LV2 = new Word(_mplc, $"D126");//A2空棧板第二層Sensor，是否有貨物在第二層 
                        }
                        else
                        {
                            buffer.A2LV2 = new Word();
                        }
                        if (bufferIndex + 1 == 4)
                        {
                            buffer.EmptyInReady = new Word(_mplc, $"D148");//A4滿版訊號 7：滿七板(即將滿板) / 8:滿8版/9:(滿9版)
                        }
                        else
                        {
                            buffer.EmptyInReady = new Word();
                        }
                        if (bufferIndex + 1 == 4)
                        {
                            buffer.EmptyError = new Word(_mplc, $"D149");//A4空棧板補充異常 1:異常 0:無異常
                        }
                        else
                        {
                            buffer.EmptyError = new Word();
                        }
                        #endregion

                        buffer.ControllerSignal.CommandId = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10)}");
                        buffer.ControllerSignal.CmdMode = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 1}");

                        #region PC->PLC增加單一特殊點位位置
                        if (bufferIndex + 1 == 1)
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word(_mplc, $"D3115");//A1出庫都要給予路徑通知，決定是否要堆疊
                            buffer.ControllerSignal.Switch_Mode = new Word(_mplc, $"D3119");//A1因為入出庫模式切換，如果可以切換，要寫入現在命令要求的模式
                        }
                        else
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word();
                            buffer.ControllerSignal.Switch_Mode = new Word();
                        }
                        if (bufferIndex + 1 == 4)
                        {
                            buffer.ControllerSignal.A4Emptysupply = new Word(_mplc, $"D3143"); //A4通知補母版
                        }
                        else 
                        {
                            buffer.ControllerSignal.A4Emptysupply = new Word();
                        }
                        if (bufferIndex + 1 == 4)
                        {
                            buffer.ControllerSignal.A4ErrorOn = new Word(_mplc, $"D3149"); //A4通知無法補充空棧板異常
                        }
                        else 
                        {
                            buffer.ControllerSignal.A4ErrorOn = new Word();
                        }
                        #endregion

                        buffer.ControllerSignal.InitialNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 9}");



                        _bufferSignals.Add(bufferIndex + 1, buffer);
                    }
        }

        public SystemSignal GetSystemSignal()
        {
            return _systemSignal;
        }
    }
}
