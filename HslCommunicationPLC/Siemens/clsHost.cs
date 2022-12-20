using HslCommunication;
using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HslCommunicationPLC.Siemens
{
    public class clsHost : IDisposable
    {
        private SiemensS7Net siemensTcpNet = null;
        private SiemensPLCS siemensPLCSelected = SiemensPLCS.S1200;
        private bool isconn = false;
        public bool IsConn
        {
            get { return isconn; }
        }

        public clsHost(PlcConfig plcConfig)
        {
            siemensPLCSelected = plcConfig.siemensPLCS;
            siemensTcpNet = new SiemensS7Net(siemensPLCSelected, plcConfig.IpAddress);
            //siemensTcpNet = new SiemensS7Net(siemensPLCSelected)
            //{
            //    IpAddress = plcConfig.IpAddress,
            //    Port = plcConfig.Port,
            //    //Rack = plcConfig.Rack,
            //    //Slot = plcConfig.Slot,
            //    //ConnectionType = plcConfig.ConnectionType,
            //    //LocalTSAP = plcConfig.LocalTSAP
            //};

            //if (siemensPLCSelected == SiemensPLCS.S400)
            //{
            //    siemensTcpNet.Slot = byte.Parse("3");
            //}
            //else if (siemensPLCSelected == SiemensPLCS.S1200)
            //{
            //    siemensTcpNet.Slot = byte.Parse("0");
            //}
            //else if (siemensPLCSelected == SiemensPLCS.S300)
            //{
            //    siemensTcpNet.Slot = byte.Parse("2");
            //}
            //else if (siemensPLCSelected == SiemensPLCS.S1500)
            //{
            //    siemensTcpNet.Slot = byte.Parse("0");
            //}
        }

        public bool Connect()
        {
            string strEM = "";
            return Connect(ref strEM);
        }

        public bool Connect(ref string strEM)
        {
            siemensTcpNet.ConnectClose();
            OperateResult connect = siemensTcpNet.ConnectServer();
            isconn = connect.IsSuccess;
            if (!isconn) strEM = connect.Message;

            return isconn;
        }

        public bool Disconnect()
        {
            OperateResult connect = siemensTcpNet.ConnectClose();
            if (connect.IsSuccess) isconn = false;

            return connect.IsSuccess;
        }

        public bool ReadBlock(string StartAddress, ref short[] Content)
        {
            string strEM = "";
            return ReadBlock(StartAddress, ref Content, ref strEM);
        }

        public bool ReadBlock(string StartAddress, ref short[] Content, ref string strEM)
        {
            OperateResult<short[]> read = siemensTcpNet.ReadInt16(StartAddress, ushort.Parse(Content.Length.ToString()));
            Content = read.Content;
            isconn = read.IsSuccess;
            if (!isconn) strEM = read.Message;

            return read.IsSuccess;
        }

        public bool WriteBlock(string StartAddress, short[] Content)
        {
            string strEM = "";
            return WriteBlock(StartAddress, Content, ref strEM);
        }

        public bool WriteBlock(string StartAddress, short[] Content, ref string strEM)
        {
            OperateResult write = siemensTcpNet.Write(StartAddress, Content);
            isconn = write.IsSuccess;
            if (!isconn) strEM = write.Message;

            return write.IsSuccess;
        }

        public bool Write(string StartAddress, short value)
        {
            string strEM = "";
            return Write(StartAddress, value, ref strEM);
        }

        public bool Write(string StartAddress, short value, ref string strEM)
        {
            OperateResult write = siemensTcpNet.Write(StartAddress, value);
            isconn = write.IsSuccess;
            if (!isconn) strEM = write.Message;

            return write.IsSuccess;
        }

        public bool WriteBit(string address, bool value)
        {
            string strEM = "";
            return WriteBit(address, value, ref strEM);
        }

        public bool WriteBit(string address, bool value, ref string strEM)
        {
            OperateResult write = siemensTcpNet.Write(address, value);
            isconn = write.IsSuccess;
            if (!isconn) strEM = write.Message;

            return write.IsSuccess;
        }

        #region IDisposeable interface implementation

        ~clsHost()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    siemensTcpNet.Dispose();
                }
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion IDisposeable interface implementation

    }
}
