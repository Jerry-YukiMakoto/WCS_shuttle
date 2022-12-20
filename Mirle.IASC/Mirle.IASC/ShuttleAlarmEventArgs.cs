
using System;

namespace Mirle.IASC
{
    public class ShuttleAlarmEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public string ErrorCode { get; }
        public string CarrierId { get; }

        public ShuttleAlarmEventArgs(string vehicleId, string errorCode, string carrierId)
        {
            VehicleId = vehicleId;
            ErrorCode = errorCode;
            CarrierId = carrierId;
        }
    }
}