
using System;

namespace Mirle.IASC
{
    public class ChangeLayerEventArgsLayer : EventArgs
    {
        public string LifterID { get; }
        public string DestinationLayer { get; }


        public ChangeLayerEventArgsLayer(string Lifter_ID, string Destination_Layer)
        {
            LifterID = Lifter_ID;
            DestinationLayer = Destination_Layer;
        }
    }
}