//WMS回覆WCS有收到回報
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Client.ReportInfo
{
    public class ReturnMsgInfo
    {
        public bool success { get; set; }
        public string errCode { get; set; }
        public string errMsg { get; set; }
    }
}
