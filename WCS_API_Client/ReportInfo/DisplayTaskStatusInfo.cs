using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Client.ReportInfo
{
    public class DisplayTaskStatusInfo
    {
        public string lineId { get; set; }
        public string locationID { get; set; }
        public string taskNo { get; set; }
        public string state { get; set; } //1: 任務開始；2:任務結束
        public string MerrCode { get; set; }
        public string MerrMsg { get; set; }
    }
}
