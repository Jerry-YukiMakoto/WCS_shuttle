namespace Mirle.IASC
{
    public class ShuttleCommand
    {
        public string CommandId { get; }
        public string CommandType { get; }
        public int Priority { get; }
        public string Source { get; }
        public string Destination { get; }
        public string CarrierId { get; }
        public string VehicleId { get; }

        public ShuttleCommand(string commandId, string commandType, int priority, string source, string destination, string carrierId, string vehicleId)
        {
            CommandId = commandId;
            CommandType = commandType;
            Priority = priority;
            Source = source;
            Destination = destination;
            CarrierId = carrierId;
            VehicleId = vehicleId;
        }
    }
}