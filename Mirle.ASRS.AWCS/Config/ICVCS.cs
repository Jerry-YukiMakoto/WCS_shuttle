
using Config.Net;

namespace Mirle.ASRS.AWCS.Config
{
    public interface ICVCS
    {
        [Option(Alias = "MPLCIP")] string MPLCIP { get; set; }
        [Option(Alias = "MPLCPort")] int MPLCPort { get; set; }
        [Option(Alias = "SignalGroup")] int SignalGroup { get; set; }
        [Option(Alias = "OnlyMonitor", DefaultValue = false)] bool OnlyMonitor { get; set; }
    }
}
