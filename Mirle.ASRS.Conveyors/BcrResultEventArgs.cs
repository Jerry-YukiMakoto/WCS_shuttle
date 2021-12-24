using System;

namespace Mirle.ASRS.Conveyors
{
    public class ReadNoticeEventArgs : BufferEventArgs
    {
        public int Weight { get; }
        public string TrayId { get; }
        public string FosbId { get; }
        public string FosbId_Left { get; }
        public string FosbId_Right { get; }
        public string Plant { get; }
        public string Plant_Left { get; }
        public string Plant_Right { get; }

        public ReadNoticeEventArgs(int bufferIndex, string bufferName, int weight, string trayId, string fosbId, string fosbId_Left, string fosbId_Right, string plant, string plant_Left, string plant_Right) : base(bufferIndex, bufferName)
        {
            Weight = weight;
            TrayId = trayId;
            FosbId = fosbId;
            FosbId_Left = fosbId_Left;
            FosbId_Right = fosbId_Right;
            Plant = plant;
            Plant_Left = plant_Left;
            Plant_Right = plant_Right;
        }

    }
}
