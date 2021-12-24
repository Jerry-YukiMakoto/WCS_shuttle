using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.ASRS.AWCS.Config
{
    public interface IWCS
    {
        IDatabase Database { get; set; }
        ICVCS CVCS { get; set; }
        ISimulator Simulator { get; set; }
    }
}
