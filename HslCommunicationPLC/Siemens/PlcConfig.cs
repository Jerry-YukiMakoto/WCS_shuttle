using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HslCommunicationPLC.Siemens
{
    public class PlcConfig
    {
        public string IpAddress { get; set; }
        public int Port { get; set; } = 0;
        //
        // 摘要:
        //     PLC的机架号，针对S7-400的PLC设置的
        //     The frame number of the PLC is set for the PLC of s7-400
        public byte Rack { get; set; } = 0;
        //
        // 摘要:
        //     PLC的槽号，针对S7-400的PLC设置的
        //     The slot number of PLC is set for PLC of s7-400
        public byte Slot { get; set; } = 0;
        //
        // 摘要:
        //     获取或设置当前PLC的连接方式，PG: 0x01，OP: 0x02，S7Basic: 0x03...0x10
        //     Get or set the current PLC connection mode, PG: 0x01, OP: 0x02, S7Basic: 0x03...0x10
        public byte ConnectionType { get; set; } = 0;
        //
        // 摘要:
        //     西门子相关的本地TSAP参数信息
        //     A parameter information related to Siemens
        public int LocalTSAP { get; set; } = 0;
        public SiemensPLCS siemensPLCS { get; set; } = SiemensPLCS.S1200;
    }
}
