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

        public SignalMapper(IMPLCProvider mplc, int signalGroup)
        {
            _mplc = mplc;
            _signalGroup = signalGroup;

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
                new BufferDefine() { BufferIndex = 1, BufferName = "A08" },
                new BufferDefine() { BufferIndex = 2, BufferName = "A09" },
                new BufferDefine() { BufferIndex = 3, BufferName = "A09-1" },
                new BufferDefine() { BufferIndex = 4, BufferName = "A09-2" },
                new BufferDefine() { BufferIndex = 5, BufferName = "A09-3" },
                new BufferDefine() { BufferIndex = 6, BufferName = "A09-4" },
                new BufferDefine() { BufferIndex = 7, BufferName = "A09-5" },
                new BufferDefine() { BufferIndex = 8, BufferName = "A10" },
                new BufferDefine() { BufferIndex = 9, BufferName = "A11" },
                new BufferDefine() { BufferIndex = 10, BufferName = "A11-1" },
                new BufferDefine() { BufferIndex = 11, BufferName = "A11-2" },
                new BufferDefine() { BufferIndex = 12, BufferName = "A12-2" },
                new BufferDefine() { BufferIndex = 13, BufferName = "A12-1" },
                new BufferDefine() { BufferIndex = 14, BufferName = "A12" },
                new BufferDefine() { BufferIndex = 15, BufferName = "A13-1" },
                new BufferDefine() { BufferIndex = 16, BufferName = "A14-1" },
                new BufferDefine() { BufferIndex = 17, BufferName = "A16-2" },
                new BufferDefine() { BufferIndex = 18, BufferName = "A16-3" },
                new BufferDefine() { BufferIndex = 19, BufferName = "A16-4" },
                new BufferDefine() { BufferIndex = 20, BufferName = "A17-2" },
                new BufferDefine() { BufferIndex = 21, BufferName = "A17-3" },
                new BufferDefine() { BufferIndex = 22, BufferName = "A17-4" },
                new BufferDefine() { BufferIndex = 23, BufferName = "A18" },
                new BufferDefine() { BufferIndex = 24, BufferName = "A19" },
                new BufferDefine() { BufferIndex = 25, BufferName = "A17-1" },
                new BufferDefine() { BufferIndex = 26, BufferName = "A17" },
                new BufferDefine() { BufferIndex = 27, BufferName = "A16-1" },
                new BufferDefine() { BufferIndex = 28, BufferName = "A16" },
                new BufferDefine() { BufferIndex = 29, BufferName = "A14" },
                new BufferDefine() { BufferIndex = 30, BufferName = "A13" },
            };
            Conveyors.Add(conveyor);

            conveyor = new ConveyorDefine();
            conveyor.SignalGroup = 1;
            conveyor.Buffers = new List<BufferDefine>()
            {
                new BufferDefine() { BufferIndex = 1, BufferName = "A01" },
                new BufferDefine() { BufferIndex = 2, BufferName = "A01-1" },
                new BufferDefine() { BufferIndex = 3, BufferName = "A01-2" },
                new BufferDefine() { BufferIndex = 4, BufferName = "A02" },
                new BufferDefine() { BufferIndex = 5, BufferName = "A02-1" },
                new BufferDefine() { BufferIndex = 6, BufferName = "A02-2" },
                new BufferDefine() { BufferIndex = 7, BufferName = "A01-5" },
                new BufferDefine() { BufferIndex = 8, BufferName = "A01-4" },
                new BufferDefine() { BufferIndex = 9, BufferName = "A01-3" },
                new BufferDefine() { BufferIndex = 10, BufferName = "A03" },
                new BufferDefine() { BufferIndex = 11, BufferName = "A03-1" },
                new BufferDefine() { BufferIndex = 12, BufferName = "A04" },
                new BufferDefine() { BufferIndex = 13, BufferName = "A05" },
                new BufferDefine() { BufferIndex = 14, BufferName = "A05-1" },
                new BufferDefine() { BufferIndex = 15, BufferName = "A07" },
                new BufferDefine() { BufferIndex = 16, BufferName = "A07-1" },
                new BufferDefine() { BufferIndex = 17, BufferName = "A03-3" },
                new BufferDefine() { BufferIndex = 18, BufferName = "A03-2" },
                new BufferDefine() { BufferIndex = 19, BufferName = "B01" },
                new BufferDefine() { BufferIndex = 20, BufferName = "B01-1" },
                new BufferDefine() { BufferIndex = 21, BufferName = "B01-2" },
                new BufferDefine() { BufferIndex = 22, BufferName = "B01-3" },
                new BufferDefine() { BufferIndex = 23, BufferName = "B01-4" },
                new BufferDefine() { BufferIndex = 24, BufferName = "B01-5" },
                new BufferDefine() { BufferIndex = 25, BufferName = "B02" },
                new BufferDefine() { BufferIndex = 26, BufferName = "B04" },
                new BufferDefine() { BufferIndex = 27, BufferName = "B04-1" },
                new BufferDefine() { BufferIndex = 28, BufferName = "B05-1" },
                new BufferDefine() { BufferIndex = 29, BufferName = "B05" },
                new BufferDefine() { BufferIndex = 30, BufferName = "B06" },
                new BufferDefine() { BufferIndex = 31, BufferName = "B07" },
                new BufferDefine() { BufferIndex = 32, BufferName = "C07" },
                new BufferDefine() { BufferIndex = 33, BufferName = "C06" },
                new BufferDefine() { BufferIndex = 34, BufferName = "C05" },
                new BufferDefine() { BufferIndex = 35, BufferName = "C04-1" },
                new BufferDefine() { BufferIndex = 36, BufferName = "C04" },
                new BufferDefine() { BufferIndex = 37, BufferName = "C05" },
                new BufferDefine() { BufferIndex = 38, BufferName = "C01-2" },
                new BufferDefine() { BufferIndex = 39, BufferName = "C01-1" },
                new BufferDefine() { BufferIndex = 40, BufferName = "C01" },
                new BufferDefine() { BufferIndex = 41, BufferName = "C01-5" },
                new BufferDefine() { BufferIndex = 42, BufferName = "C01-4" },
                new BufferDefine() { BufferIndex = 43, BufferName = "C01-3" },
                new BufferDefine() { BufferIndex = 44, BufferName = "B09" },
                new BufferDefine() { BufferIndex = 45, BufferName = "B11" },
                new BufferDefine() { BufferIndex = 46, BufferName = "B11-1" },
                new BufferDefine() { BufferIndex = 47, BufferName = "B12" },
                new BufferDefine() { BufferIndex = 48, BufferName = "B12-1" },
                new BufferDefine() { BufferIndex = 49, BufferName = "B12-5" },
                new BufferDefine() { BufferIndex = 50, BufferName = "B12-2" },
                new BufferDefine() { BufferIndex = 51, BufferName = "B13" },
                new BufferDefine() { BufferIndex = 52, BufferName = "B13-1" },
                new BufferDefine() { BufferIndex = 53, BufferName = "B13-2" },
                new BufferDefine() { BufferIndex = 54, BufferName = "B14" },
                new BufferDefine() { BufferIndex = 55, BufferName = "B14-1" },
                new BufferDefine() { BufferIndex = 56, BufferName = "B14-5" },
                new BufferDefine() { BufferIndex = 57, BufferName = "B14-2" },
                new BufferDefine() { BufferIndex = 58, BufferName = "B15" },
                new BufferDefine() { BufferIndex = 59, BufferName = "B15-4" },
                new BufferDefine() { BufferIndex = 60, BufferName = "B15-1" },
                new BufferDefine() { BufferIndex = 61, BufferName = "B16" },
                new BufferDefine() { BufferIndex = 62, BufferName = "B16-1" },
                new BufferDefine() { BufferIndex = 63, BufferName = "B16-5" },
                new BufferDefine() { BufferIndex = 64, BufferName = "B16-2" },
                new BufferDefine() { BufferIndex = 65, BufferName = "B17" },
                new BufferDefine() { BufferIndex = 66, BufferName = "B18" },
                new BufferDefine() { BufferIndex = 67, BufferName = "B16-4" },
                new BufferDefine() { BufferIndex = 68, BufferName = "B16-3" },
                new BufferDefine() { BufferIndex = 69, BufferName = "B15-3" },
                new BufferDefine() { BufferIndex = 70, BufferName = "B15-2" },
                new BufferDefine() { BufferIndex = 71, BufferName = "B14-4" },
                new BufferDefine() { BufferIndex = 72, BufferName = "B14-3" },
                new BufferDefine() { BufferIndex = 73, BufferName = "B13-4" },
                new BufferDefine() { BufferIndex = 74, BufferName = "B13-3" },
                new BufferDefine() { BufferIndex = 75, BufferName = "B12-4" },
                new BufferDefine() { BufferIndex = 76, BufferName = "B12-3" },
                new BufferDefine() { BufferIndex = 77, BufferName = "B11-3" },
                new BufferDefine() { BufferIndex = 78, BufferName = "B11-2" },
                new BufferDefine() { BufferIndex = 79, BufferName = "B10" },
            };
            Conveyors.Add(conveyor);
            return Conveyors;
        }

        private void MappingSystem()
        {
            int plcIndex = 1001;
            int pcIndex = 3001;
            _systemSignal.Heartbeat = new Word(_mplc, $"D{plcIndex}");
            _systemSignal.Alarm = new Word(_mplc, $"D{plcIndex + 6}");

            _systemSignal.ControllerSignal.Heartbeat = new Word(_mplc, $"D{pcIndex}");
            _systemSignal.ControllerSignal.SystemTimeCalibration = new WordBlock(_mplc, $"D{plcIndex + 1}", 6);
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

            int plcIndex = 1011;
            int pcIndex = 3011;

            if (define.Exists(r => r.SignalGroup == _signalGroup))
            {
                var conveyor = define.Find(r => r.SignalGroup == _signalGroup);
                if (conveyor.SignalGroup == 0)
                {
                    var readyBufferIndex = new Dictionary<int, int>();
                    readyBufferIndex.Add(1, 0);//A08
                    readyBufferIndex.Add(11, 10); //A11-2
                    readyBufferIndex.Add(14, 13);//A12
                    readyBufferIndex.Add(15, 14); //A13-1
                    readyBufferIndex.Add(20, 19); //A17-2
                    readyBufferIndex.Add(21, 20);//A17-3
                    readyBufferIndex.Add(24, 23);//A19
                    readyBufferIndex.Add(30, 29);//A13

                    var readNoticeBufferIndex = new Dictionary<int, int>();
                    readNoticeBufferIndex.Add(11, 10);//A11-2
                    readNoticeBufferIndex.Add(21, 20);//A17-3
                    readNoticeBufferIndex.Add(24, 23);//A19
                    readNoticeBufferIndex.Add(30, 29);//A13

                    var automaticDoorBufferIndex = new Dictionary<int, int>();
                    automaticDoorBufferIndex.Add(8, 7);//A10

                    var dataBufferIndex = new Dictionary<int, int>();
                    dataBufferIndex.Add(11, 2001);//A11-2
                    dataBufferIndex.Add(21, 2024);//A17-3
                    dataBufferIndex.Add(24, 2061);//A19
                    dataBufferIndex.Add(30, 2082);//A13

                    var pathChangeNotice = new Dictionary<int, int>();
                    pathChangeNotice.Add(20, 19);//A17-2

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
                        buffer.PathNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 1}");
                        buffer.LoadCategory = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}");

                        buffer.StatusSignal.InMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.1");
                        buffer.StatusSignal.OutMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.2");
                        buffer.StatusSignal.Error = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.3");
                        buffer.StatusSignal.Auto = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.4");
                        buffer.StatusSignal.Manual = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.5");
                        buffer.StatusSignal.Presence = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.6");
                        buffer.StatusSignal.Position = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.7");
                        buffer.StatusSignal.Finish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.8");
                        buffer.StatusSignal.EmergencyStop = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.9");
                        buffer.StatusSignal.AutomaticDoor = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.A");
                        buffer.StatusSignal.LoadReq = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.B");
                        buffer.StatusSignal.LoadFinish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.C");
                        buffer.StatusSignal.UnloadReq = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.D");
                        buffer.StatusSignal.UnloadFinish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.E");

                        buffer.Alarm = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 4}");

                        if (readyBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.Ready = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 5}");
                        }
                        else
                        {
                            buffer.Ready = new Word();
                        }

                        if (readNoticeBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ReadNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 6}");
                            buffer.EmptyBoxCount = new Word();
                        }
                        else
                        {
                            buffer.ReadNotice = new Word();
                            buffer.EmptyBoxCount = new Word();
                        }

                        buffer.TrayType = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 7}");
                        buffer.PickingDirection = new Word();
                        buffer.InitialNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 9}");

                        buffer.ControllerSignal.CommandData = new WordBlock(_mplc, $"D{pcIndex + (bufferIndex * 10)}", 3);

                        buffer.ControllerSignal.CommandId = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10)}");
                        buffer.ControllerSignal.PathNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 1}");
                        buffer.ControllerSignal.LoadCategory = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 2}");

                        if (readNoticeBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 5}");
                        }
                        else
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word();
                        }

                        buffer.ControllerSignal.TrayType = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 7}");

                        if (automaticDoorBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ControllerSignal.AutomaticDoorClosed = new Bit(_mplc, $"D{pcIndex + (bufferIndex * 10) + 8}.0");
                            buffer.ControllerSignal.AutomaticDoorOpend = new Bit(_mplc, $"D{pcIndex + (bufferIndex * 10) + 8}.1");
                        }
                        else
                        {
                            buffer.ControllerSignal.AutomaticDoorClosed = new Bit();
                            buffer.ControllerSignal.AutomaticDoorOpend = new Bit();
                        }

                        buffer.ControllerSignal.InitialNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 9}");

                        if (dataBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            int index = dataBufferIndex[bufferIndex + 1];
                            if (bufferIndex + 1 == 11)
                            {
                                buffer.DataBuffer.Weight = new DWord(_mplc, $"D{index}");
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index + 2}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 7}", 16);
                                buffer.DataBuffer.Plant = new WordBlock();
                            }
                            else if (bufferIndex + 1 == 21)
                            {
                                buffer.DataBuffer.Weight = new DWord();
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 5}", 16);
                                buffer.DataBuffer.Plant = new WordBlock(_mplc, $"D{index + 21}", 6);
                            }
                            else
                            {
                                buffer.DataBuffer.Weight = new DWord();
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 5}", 16);
                                buffer.DataBuffer.Plant = new WordBlock();
                            }
                            buffer.DataBuffer.Plant_Left = new WordBlock();
                            buffer.DataBuffer.Plant_Right = new WordBlock();
                            buffer.DataBuffer.FosbId_Left = new WordBlock();
                            buffer.DataBuffer.FosbId_Right = new WordBlock();
                        }
                        else
                        {
                            buffer.DataBuffer.Weight = new DWord();
                            buffer.DataBuffer.TrayId = new WordBlock();
                            buffer.DataBuffer.FosbId = new WordBlock();
                            buffer.DataBuffer.Plant = new WordBlock();
                            buffer.DataBuffer.Plant_Left = new WordBlock();
                            buffer.DataBuffer.Plant_Right = new WordBlock();
                            buffer.DataBuffer.FosbId_Left = new WordBlock();
                            buffer.DataBuffer.FosbId_Right = new WordBlock();
                        }

                        _bufferSignals.Add(bufferIndex + 1, buffer);
                    }
                }
                else
                {
                    var readyBufferIndex = new Dictionary<int, int>();
                    readyBufferIndex.Add(1, 0);//A01
                    readyBufferIndex.Add(6, 5);//A02-2
                    readyBufferIndex.Add(9, 8);//A01-3
                    readyBufferIndex.Add(10, 9);//A03
                    readyBufferIndex.Add(15, 14);//A07
                    readyBufferIndex.Add(18, 17);//A03-2
                    readyBufferIndex.Add(19, 18);//B01
                    readyBufferIndex.Add(21, 20);//B01-2
                    readyBufferIndex.Add(22, 21);//B01-3
                    readyBufferIndex.Add(24, 23);//B01-5
                    readyBufferIndex.Add(25, 24);//B02
                    readyBufferIndex.Add(37, 36);//C02
                    readyBufferIndex.Add(38, 37);//C01-2
                    readyBufferIndex.Add(40, 39);//C01
                    readyBufferIndex.Add(41, 40);//C01-5
                    readyBufferIndex.Add(43, 42);//C01-3

                    var readNoticeBufferIndex = new Dictionary<int, int>();
                    readNoticeBufferIndex.Add(6, 5);//A02-2
                    readNoticeBufferIndex.Add(15, 14);//A07

                    var automaticDoorBufferIndex = new Dictionary<int, int>();

                    var dataBufferIndex = new Dictionary<int, int>();

                    var pathChangeNotice = new Dictionary<int, int>();

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
                        buffer.PathNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 1}");
                        buffer.LoadCategory = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 2}");

                        buffer.StatusSignal.InMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.1");
                        buffer.StatusSignal.OutMode = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.2");
                        buffer.StatusSignal.Error = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.3");
                        buffer.StatusSignal.Auto = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.4");
                        buffer.StatusSignal.Manual = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.5");
                        buffer.StatusSignal.Presence = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.6");
                        buffer.StatusSignal.Position = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.7");
                        buffer.StatusSignal.Finish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.8");
                        buffer.StatusSignal.EmergencyStop = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.9");
                        buffer.StatusSignal.AutomaticDoor = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.A");
                        buffer.StatusSignal.LoadReq = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.B");
                        buffer.StatusSignal.LoadFinish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.C");
                        buffer.StatusSignal.UnloadReq = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.D");
                        buffer.StatusSignal.UnloadFinish = new Bit(_mplc, $"D{plcIndex + (bufferIndex * 10) + 3}.E");

                        buffer.Alarm = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 4}");

                        if (readyBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.Ready = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 5}");
                        }
                        else
                        {
                            buffer.Ready = new Word();
                        }

                        if (readNoticeBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ReadNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 6}");
                            buffer.EmptyBoxCount = new Word();
                        }
                        else
                        {
                            buffer.ReadNotice = new Word();
                            buffer.EmptyBoxCount = new Word();
                        }

                        buffer.TrayType = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 7}");
                        buffer.PickingDirection = new Word();
                        buffer.InitialNotice = new Word(_mplc, $"D{plcIndex + (bufferIndex * 10) + 9}");

                        buffer.ControllerSignal.CommandData = new WordBlock(_mplc, $"D{pcIndex + (bufferIndex * 10)}", 3);

                        buffer.ControllerSignal.CommandId = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10)}");
                        buffer.ControllerSignal.PathNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 1}");
                        buffer.ControllerSignal.LoadCategory = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 2}");

                        if (readNoticeBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 5}");
                        }
                        else
                        {
                            buffer.ControllerSignal.PathChangeNotice = new Word();
                        }

                        buffer.ControllerSignal.TrayType = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 7}");

                        if (automaticDoorBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            buffer.ControllerSignal.AutomaticDoorClosed = new Bit(_mplc, $"D{pcIndex + (bufferIndex * 10) + 8}.0");
                            buffer.ControllerSignal.AutomaticDoorOpend = new Bit(_mplc, $"D{pcIndex + (bufferIndex * 10) + 8}.1");
                        }
                        else
                        {
                            buffer.ControllerSignal.AutomaticDoorClosed = new Bit();
                            buffer.ControllerSignal.AutomaticDoorOpend = new Bit();
                        }

                        buffer.ControllerSignal.InitialNotice = new Word(_mplc, $"D{pcIndex + (bufferIndex * 10) + 9}");

                        if (dataBufferIndex.ContainsKey(bufferIndex + 1))
                        {
                            int index = dataBufferIndex[bufferIndex + 1];
                            if (bufferIndex + 1 == 11)
                            {
                                buffer.DataBuffer.Weight = new DWord(_mplc, $"D{index}");
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index + 2}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 7}", 16);
                                buffer.DataBuffer.Plant = new WordBlock();
                            }
                            else if (bufferIndex + 1 == 21)
                            {
                                buffer.DataBuffer.Weight = new DWord();
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 5}", 16);
                                buffer.DataBuffer.Plant = new WordBlock(_mplc, $"D{index + 21}", 6);
                            }
                            else
                            {
                                buffer.DataBuffer.Weight = new DWord();
                                buffer.DataBuffer.TrayId = new WordBlock(_mplc, $"D{index}", 5);
                                buffer.DataBuffer.FosbId = new WordBlock(_mplc, $"D{index + 5}", 16);
                                buffer.DataBuffer.Plant = new WordBlock();
                            }
                            buffer.DataBuffer.Plant_Left = new WordBlock();
                            buffer.DataBuffer.Plant_Right = new WordBlock();
                            buffer.DataBuffer.FosbId_Left = new WordBlock();
                            buffer.DataBuffer.FosbId_Right = new WordBlock();
                        }
                        else
                        {
                            buffer.DataBuffer.Weight = new DWord();
                            buffer.DataBuffer.TrayId = new WordBlock();
                            buffer.DataBuffer.FosbId = new WordBlock();
                            buffer.DataBuffer.Plant = new WordBlock();
                            buffer.DataBuffer.Plant_Left = new WordBlock();
                            buffer.DataBuffer.Plant_Right = new WordBlock();
                            buffer.DataBuffer.FosbId_Left = new WordBlock();
                            buffer.DataBuffer.FosbId_Right = new WordBlock();
                        }

                        _bufferSignals.Add(bufferIndex + 1, buffer);
                    }
                }
            }
            else
            {
            }
        }

        public SystemSignal GetSystemSignal()
        {
            return _systemSignal;
        }
    }
}
