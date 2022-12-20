
using System;

namespace Mirle.IASC
{
    public class QueryCommandStatusEventArgs : EventArgs
    {
        public string CommandId { get; }
        public string VehicleId { get; }
        public string CommandStatus { get; }

        public QueryCommandStatusEventArgs(string commandId, string vehicleId, string commandStatus)
        {
            CommandId = commandId;
            VehicleId = vehicleId;
            CommandStatus = commandStatus;
        }
    }
}
