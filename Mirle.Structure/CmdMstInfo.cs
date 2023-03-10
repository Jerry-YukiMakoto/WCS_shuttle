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
        public string TaskNo { get; set; }
        public string CmdSts { get; set; }
        public string Prt { get; set; }
        public string StnNo { get; set; }
        public string CmdMode { get; set; }
        public string IoType { get; set; }
        public string Loc { get; set; }

        public string Loc_ID { get; set; }
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
        public string EquNo { get; set; }
        /// <summary>
        /// 當前位置
        /// </summary>
        public string taskNo { get; set; }
        public string TrnUser { get; set; }
        public string Remark { get; set; }

    }
}
