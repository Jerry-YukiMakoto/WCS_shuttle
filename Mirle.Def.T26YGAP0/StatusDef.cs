using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Def.T26YGAP0
{
    public class StatusDef
    {
        /// <summary>
        /// 搬運結束
        /// </summary>
        public const string moveTaskFinished = "11";
        /// <summary>
        /// 下達命令
        /// </summary>
        public const string cmdDelivered = "12";
        /// <summary>
        /// 正常完成
        /// </summary>
        public const string normalFinished = "13";
        /// <summary>
        /// 強制完成
        /// </summary>
        public const string forceFinished = "14";
        /// <summary>
        /// 強制取消
        /// </summary>
        public const string forceCanceled = "15";
        /// <summary>
        /// 任務異常
        /// </summary>
        public const string taskError = "9999";
    }
}
