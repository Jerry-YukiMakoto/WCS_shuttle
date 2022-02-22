using System;
using System.Collections.Generic;
using System.Text;

namespace test
{
    public class MoveTaskAddInfo
    {
        public string lineId { get; set; } = "1"; //線別
        public string taskNo { get; set; } //任務號
        public string bussinessType { get; set; } //任務類型
        public string locationFrom { get; set; } //起始儲位(站口)
        public string locationTo { get; set; } //目標儲位(站口)
        public string priority { get; set; } //優先權
        public string deliveryTime { get; set; } //任務下達時間
    }
}
