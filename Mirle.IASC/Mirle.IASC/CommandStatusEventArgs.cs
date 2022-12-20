
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.IASC
{
    public class CommandStatusEventArgs : EventArgs
    {
        public string CommandId { get; }
        public string VehicleId { get; }
        public string CommandStatus { get; }
        public string ResultCode { get; }

        public CommandStatusEventArgs(string commandId, string vehicleId, string commandStatus, string resultCode)
        {
            CommandId = commandId;
            VehicleId = vehicleId;
            CommandStatus = commandStatus;
            ResultCode = resultCode;
        }
    }
}
