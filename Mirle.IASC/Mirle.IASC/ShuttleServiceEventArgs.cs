
using System;
using System.Collections.Generic;

namespace Mirle.IASC
{
    public class ShuttleServiceEventArgs : EventArgs
    {
        public int Layer { get; }
        public int AreaCount { get; }
        public IEnumerable<bool> ServiceStatus { get; }

        public ShuttleServiceEventArgs(int layer, int areaCount, IEnumerable<bool> serviceStatus)
        {
            Layer = layer;
            AreaCount = areaCount;
            ServiceStatus = serviceStatus;
        }
    }
}