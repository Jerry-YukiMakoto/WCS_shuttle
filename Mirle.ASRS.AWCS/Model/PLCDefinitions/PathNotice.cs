using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.ASRS.AWCS.Model.PLCDefinitions
{
    public struct PathNotice
    {
        public const int Path1_toA2 = 1; //編號1為堆疊
        public const int Path2_toA3 = 2; //編號2為直接出庫
        public const int Path3_toA4 = 3; //編號三為補充母棧板
    }
}