using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Client.ReportInfo
{
    public class TaskStateUpdateReturnInfo : ReturnMsgInfo
    {
        public string lineId { get; set; }
        public string taskNo { get; set; }
    }
}
