using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Client.ReportInfo
{
    public class TaskStateUpdateInfo
    {
        public string lineId { get; set; } = "1";
        public string taskNo { get; set; }
        public string palletNo { get; set; }
        public string bussinessType { get; set; }
        public string state { get; set; }
        public string errMsg { get; set; }
    }
}
