using System;
using System.Collections.Generic;
using System.Linq;

using Mirle.ASRS.Conveyors;
using Mirle.ASRS.Conveyors.Signal;
using Mirle.MPLC;
using Mirle.MPLC.DataBlocks;
using Mirle.MPLC.DataBlocks.DeviceRange;
using Mirle.MPLC.MCProtocol;
using Mirle.MPLC.SharedMemory;

namespace Mirle.ASRS.WCS.Controller
{
    public class CVController : IDisposable
    {
        private readonly PLCHost _plcHost;
        private readonly Conveyor _converyor;

        public CVController(string ipAddress, int tcpPort, int signalGroup, bool simulatorEnable)
        {
            if (simulatorEnable)
            {
                var smWriter = new SMReadWriter();
                var blockInfos = GetBlockInfos(signalGroup);
                foreach (var block in blockInfos)
                {
                    smWriter.AddDataBlock(new SMDataBlockInt32(block.DeviceRange, $@"Global\{block.SharedMemoryName}"));
                }
                _converyor = new Conveyor(smWriter, signalGroup);
            }
            else
            {
                var plcHostInfo = new PLCHostInfo();
                plcHostInfo.IPAddress = ipAddress;
                plcHostInfo.TcpPort = tcpPort;
                plcHostInfo.HostId = signalGroup == 0 ? "BQA" : "MFG";
                plcHostInfo.BlockInfos = GetBlockInfos(signalGroup);
                _plcHost = new PLCHost(plcHostInfo);
                _plcHost.Interval = 200;
                _plcHost.MPLCTimeout = 600;
                _plcHost.EnableWriteRawData = true;
                _plcHost.EnableWriteShareMemory = true;
                var smReader = new SMReadOnlyCachedReader();
                var blockInfos = GetBlockInfos(signalGroup);
                foreach (var block in blockInfos)
                {
                    smReader.AddDataBlock(new SMDataBlockInt32(block.DeviceRange, $@"Global\{block.SharedMemoryName}"));
                }

                _converyor = new Conveyor(_plcHost, signalGroup);
                _plcHost.Start();
            }

            _converyor.OnSystemAlarmClear += Converyor_OnSystemAlarmClear;
            _converyor.OnSystemAlarmTrigger += Converyor_OnSystemAlarmTrigger;
            foreach (var buffer in _converyor.Buffers)
            {
                buffer.OnAlarmClear += Buffer_OnAlarmClear;
                buffer.OnAlarmTrigger += Buffer_OnAlarmTrigger;
                buffer.OnAutomaticDoorClose += Buffer_OnAutomaticDoorClose;
                buffer.OnAutomaticDoorOpend += Buffer_OnAutomaticDoorOpend;
                buffer.OnIniatlNotice += Buffer_OnIniatlNotice;
                buffer.OnIniatlNoticeComplete += Buffer_OnIniatlNoticeComplete;
                buffer.OnReadNotice += Buffer_OnReadNotice;
            }

            _converyor.Start();
        }

        private IEnumerable<BlockInfo> GetBlockInfos(int signalGroup)
        {
            if (signalGroup == 0)
            {
                yield return new BlockInfo(new DDeviceRange("D1001", "D1310"), "Read", 0);
                yield return new BlockInfo(new DDeviceRange("D2001", "D2102"), "DataBuffer", 1);
                yield return new BlockInfo(new DDeviceRange("D3001", "D3310"), "Write", 2);
            }
            else
            {
                yield return new BlockInfo(new DDeviceRange("D1001", "D1310"), "Read", 0);
                yield return new BlockInfo(new DDeviceRange("D2001", "D2102"), "DataBuffer", 1);
                yield return new BlockInfo(new DDeviceRange("D3001", "D3310"), "Write", 2);
            }
        }

        private void Converyor_OnSystemAlarmTrigger(object sender, AlarmEventArgs e)
        {
        }

        private void Converyor_OnSystemAlarmClear(object sender, AlarmEventArgs e)
        {
        }

        private void Buffer_OnReadNotice(object sender, ReadNoticeEventArgs e)
        {
        }

        private void Buffer_OnIniatlNoticeComplete(object sender, BufferEventArgs e)
        {
        }

        private void Buffer_OnIniatlNotice(object sender, BufferEventArgs e)
        {
        }

        private void Buffer_OnAutomaticDoorOpend(object sender, BufferEventArgs e)
        {
        }

        private void Buffer_OnAutomaticDoorClose(object sender, BufferEventArgs e)
        {
        }

        private void Buffer_OnAlarmTrigger(object sender, AlarmEventArgs e)
        {
        }

        private void Buffer_OnAlarmClear(object sender, AlarmEventArgs e)
        {
        }

        public Conveyor GetConveryor()
        {
            return _converyor;
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _converyor.Dispose();
                    _plcHost.Dispose();
                }

                disposedValue = true;
            }
        }

        ~CVController()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
