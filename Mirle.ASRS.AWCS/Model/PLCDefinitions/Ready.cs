using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.ASRS.AWCS.Model.PLCDefinitions
{
    public struct Ready
    {
        public const int NoReady = 0;
        public const int StoreInReady = 1;
        public const int StoreOutReady = 2;
    }
}
