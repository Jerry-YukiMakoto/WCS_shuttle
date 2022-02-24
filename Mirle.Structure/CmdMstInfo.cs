using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Structure
{
    public class CmdMstInfo
    {
        public string CmdSno { get; set; }
        public string CmdSts { get; set; }
        public string Prt { get; set; }
        public string StnNo { get; set; }
        public string CmdMode { get; set; }
        public string IoType { get; set; }
        public string Loc { get; set; }
        public string NewLoc { get; set; }
        /// <summary>
        /// 命令產生時間
        /// </summary>
        public string CrtDate { get; set; }
        /// <summary>
        /// 命令送出時間
        /// </summary>
        public string ExpDate { get; set; }
        public string EndDate { get; set; }
        public string Userid { get; set; }
        public string BoxID { get; set; }
        public string EquNo { get; set; }
        /// <summary>
        /// 當前位置
        /// </summary>
        public string CurLoc { get; set; }
        public string CurDeviceID { get; set; }
        public string taskNo { get; set; }
        //public string JobID { get; set; }
        public string BatchID { get; set; }
        public string ZoneID { get; set; }
        public string Remark { get; set; }
        public string NeedShelfToShelf { get; set; }
        public string backupPortId { get; set; }
        public string ticketId { get; set; }
        public string manualStockIn { get; set; }
        /// <summary>
        /// 整板出庫
        /// </summary>
        public string WhetherAllout { get; set; }
        /// <summary>
        /// 是否是尾板
        /// </summary>
        public string lastPallet { get; set; }

    }
}
