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
        /// <summary>
        /// 是否整板出庫
        /// 值為 0：非整板(檢料)
        /// 值為 1：整板
        /// </summary>
        public string WhetherAllout { get; set; }
        /// <summary>
        /// 是否尾數
        /// 目標站口是1樓時必需
        /// 0：非尾數，1：尾數
        /// </summary>
        public string lastPallet { get; set; }

        public string deliveryTime { get; set; } //任務下達時間
    }
}
