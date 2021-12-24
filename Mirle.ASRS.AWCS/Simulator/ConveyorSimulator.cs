using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.MPLC;

namespace Mirle.ASRS.AWCS.Simulator
{
    public class ConveyorSimulator
    {
        private readonly IMPLCProvider _mplc;

        public ConveyorSimulator(IMPLCProvider mplc)
        {
            _mplc = mplc;
        }


    }
}
