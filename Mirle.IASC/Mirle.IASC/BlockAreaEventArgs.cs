
using System;

namespace Mirle.IASC
{
    public class AreaEventArgs : EventArgs
    {
        public string AreaId { get; }
        public AreaEventArgs(string areaId)
        {
            AreaId = areaId;
        }
    }
}
