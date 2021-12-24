
using Config.Net;

namespace Mirle.ASRS.AWCS.Config
{
    public interface ISimulator
    {
        [Option(Alias = "Enable", DefaultValue = false)] bool Enable { get; set; }
        [Option(Alias = "UseSharedMemory", DefaultValue = true)] bool UseSharedMemory { get; set; }
    }
}
