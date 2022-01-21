using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mirle.ASRS.Conveyors;
using Mirle.ASRS.Conveyors.View;
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
        private readonly MainView _mainView;
        private readonly Conveyors.Conveyor _converyor;
        private readonly bool _InMemorySimulator;
        private readonly bool _PlcConnected;

        public CVController(string ipAddress, int tcpPort, bool InMemorySimulator)
        {
            if (InMemorySimulator)
            {
                var smWriter = new SMReadWriter();
                var blockInfos = GetBlockInfos();
                foreach (var block in blockInfos)
                {
                    smWriter.AddDataBlock(new SMDataBlockInt32(block.DeviceRange, $@"Global\{block.SharedMemoryName}"));
                }
                _converyor = new Conveyors.Conveyor(smWriter);
                _InMemorySimulator = InMemorySimulator;
            }
            else
            {
                var plcHostInfo = new PLCHostInfo("Gold", ipAddress, tcpPort, GetBlockInfos());
                _plcHost = new PLCHost(plcHostInfo);
                _plcHost.Interval = 200;
                _plcHost.MPLCTimeout = 600;
                _plcHost.EnableWriteRawData = true;
                _plcHost.EnableWriteShareMemory = true;
                var smReader = new SMReadOnlyCachedReader();
                var blockInfos = GetBlockInfos();
                foreach (var block in blockInfos)
                {
                    smReader.AddDataBlock(new SMDataBlockInt32(block.DeviceRange, $@"Global\{block.SharedMemoryName}"));
                }
                _converyor = new Conveyors.Conveyor(_plcHost);
                _plcHost.Start();
            }

            foreach (var buffer in _converyor.Buffers)
            {
                buffer.OnIniatlNotice += Buffer_OnIniatlNotice;
            }

            _converyor.Start();
            _PlcConnected = _plcHost.IsConnected;
            _mainView = new MainView(_converyor, _PlcConnected);
        }

        private IEnumerable<BlockInfo> GetBlockInfos()
        {    
                yield return new BlockInfo(new DDeviceRange("D101", "D210"), "Read", 0);
                yield return new BlockInfo(new DDeviceRange("D3101", "D3210"), "Write", 1);
        }




        private void Buffer_OnIniatlNotice(object sender, BufferEventArgs e)
        {
        }

      

        public Conveyors.Conveyor GetConveryor()
        {
            return _converyor;
        }

        public bool GetConnect()
        {
            if (_InMemorySimulator)
            {
                return true;
            }
            else
            {
                return _plcHost.IsConnected;
            }
        }

        public Form GetMainView()
        {
            return _mainView;
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
