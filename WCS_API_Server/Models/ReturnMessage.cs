using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_API_Server.Models
{
    public class ReturnMessage
    {
        public string lineId { get; set; } //倉別
        public string taskNo { get; set; } //任務號
        public bool success { get; set; } //true: 調用成功；false: 失敗
        public string errCode { get; set; } //異常碼
        public string errMsg { get; set; } //錯誤說明(在success值為false，errCode有值的情況)
    }
}
