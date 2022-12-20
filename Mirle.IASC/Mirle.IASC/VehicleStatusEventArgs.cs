
using System;

namespace Mirle.IASC
{
    public class VehicleStatusEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public string VehicleLocatedLayer { get; }
        public string VehicleStatus { get; }

        public VehicleStatusEventArgs(string vehicleId, string vehicleLocatedLayer, string vehicleStatus)
        {
            VehicleId = vehicleId;
            VehicleLocatedLayer = vehicleLocatedLayer;
            VehicleStatus = vehicleStatus;
        }
    }
}
