using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Server.Models
{
    public class MoveTaskAddInfo : BaseInfo
    {
        public string bussinessType { get; set; } //任務類型
        public string locationFrom { get; set; } //起始儲位(站口)
        public string locationTo { get; set; } //目標儲位(站口)
        public string priority { get; set; } //優先權
        public string deliveryTime { get; set; } //任務下達時間
    }
}
