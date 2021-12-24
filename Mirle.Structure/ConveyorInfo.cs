using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.Def;

namespace Mirle.Structure
{
    public class ConveyorInfo
    {
        public string StnNo { get; set; } = "";
        public int Path { get; set; } = 0;
        public int StkPortID { get; set; } = 0;
        public int Index { get; set; } = 0;
        public string BufferName { get; set; } = "";
        public Location bufferLocation { get; set; }
        public int WaterLevel { get; set; } = 0;
    }
}
