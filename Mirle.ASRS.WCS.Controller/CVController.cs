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

            foreach (var buffer in _converyor.Buffers)
            {
                buffer.OnIniatlNotice += Buffer_OnIniatlNotice;
            }

            _converyor.Start();
        }

        private IEnumerable<BlockInfo> GetBlockInfos(int signalGroup)
        {
            if (signalGroup == 0)
            {
                yield return new BlockInfo(new DDeviceRange("D101", "D210"), "Read", 0);
                yield return new BlockInfo(new DDeviceRange("D3101", "D3210"), "Write", 1);
            }
            else
            {
                yield return new BlockInfo(new DDeviceRange("D101", "D210"), "Read", 0);
                yield return new BlockInfo(new DDeviceRange("D3101", "D3210"), "Write", 1);
            }
        }




        private void Buffer_OnIniatlNotice(object sender, BufferEventArgs e)
        {
        }

      

        public Conveyor GetConveryor()
        {
            return _converyor;
        }

        public bool GetConnect()
        {
            return _plcHost.IsConnected;
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
