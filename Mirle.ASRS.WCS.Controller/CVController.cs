using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HslCommunicationPLC.Siemens;
using Mirle.MPLC;
using Mirle.MPLC.DataBlocks;
using Mirle.MPLC.DataBlocks.DeviceRange;
using Mirle.MPLC.MCProtocol;
using Mirle.MPLC.SharedMemory;
using PLCConfigSetting.PLCsetting;

namespace Mirle.ASRS.WCS.Controller
{
    public class CVController : IDisposable
    {
        private readonly clsHost _plcHost;
        private readonly bool _InMemorySimulator;
        public static clsBufferData Plc1 = new clsBufferData();


        public CVController(PlcConfig CVConfig)
        {
            _plcHost = new clsHost(CVConfig);
            Plc1.subStart(_plcHost, CVConfig);
        }






        public clsBufferData GetPLC1()
        {
            return Plc1;
        }



        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                  
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
