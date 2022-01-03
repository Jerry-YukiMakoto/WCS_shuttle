using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Def
{
    public class clsPlcConfig
    {
        public int MPLCNo { get; set; }
        public string MPLCIP { get; set; }
        public int MPLCPort { get; set; }
        public int MPLCTimeout { get; set; }
        public bool WritePLCRawData { get; set; }
        public bool UseMCProtocol { get; set; }
        public bool InMemorySimulator { get; set; }
        public int SignalGroup { get; set; }

        /// <summary>
        /// (選用) 大循環的最多荷有數
        /// </summary>
        public int CycleCount_Max { get; set; } = 0;
    }
}
