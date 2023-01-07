using System;
using System.Collections.Generic;
using System.Timers;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.WCS.Controller;
using Mirle.DataBase;
using Mirle.DB.Object.Service;
using Mirle.Def;
using Mirle.DB.Proc;
using HslCommunicationPLC;
using HslCommunicationPLC.Siemens;
using Mirle.IASC;
using Mirle.BarcodeReader;

namespace Mirle.ASRS.WCS
{
    public class WCSManager
    {
        private readonly LoggerManager _loggerManager;
        private bool IsConnected = false;
        public static clsBufferData Plc = new clsBufferData();

        public WCSManager()
        {

            _loggerManager = ControllerReader.GetLoggerManager();

        }

        public void WCSManagerControl(clsBufferData Plc1)
        {
            Plc1 = Plc;
            IsConnected = Plc1.bConnectPLC;
            if (IsConnected)
            {
                clsStoreIn.StoreIn_WriteCV(Plc1);//一樓入庫PLC1寫入命令

                clsStoreIn.StoreIn_CALL_LifterAndSHC(Plc1);//一樓入庫開始=>lifter與SHC交握 

                clsStoreIn.StoreIn_CarInLifter_WriteCmdInLifter(Plc1);

                clsStoreOut.StoreOut_WriteCV(Plc1);

                clsOther.Fun_L2L(Plc1);

            }

        }

        public void WCSManageControlSHC_Call(ChangeLayerEventArgsLayer e)
        {
            IsConnected = Plc.bConnectPLC;
            if (IsConnected)
            {
                clsStoreIn.FunSHC_ChangeLayerReq(Plc, e);
            }

        }

        public void WCSManageControlBCRSocket_Call(SocketDataReceiveEventArgs e)
        {
            IsConnected = Plc.bConnectPLC;
            if (IsConnected)
            {
                clsStoreIn.FunBCR_StoreINSocket(Plc, e);
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

                }

                disposedValue = true;
            }
        }

        ~WCSManager()
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