
using System;

namespace Mirle.IASC
{
    public class UnknowCarrierOnVehicleEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public string CarrierId { get; }

        public UnknowCarrierOnVehicleEventArgs(string vehicleId, string carrierId)
        {
            VehicleId = vehicleId;
            CarrierId = carrierId;
        }
    }
}
