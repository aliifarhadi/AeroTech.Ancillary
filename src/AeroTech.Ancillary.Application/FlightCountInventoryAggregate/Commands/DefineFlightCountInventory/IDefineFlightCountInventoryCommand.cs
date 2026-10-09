using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    public interface IDefineFlightCountInventoryCommand
    {
        int OwnerAirlineId { get; }

        long FlightId { get; }

        long ResourceId { get; }

        InventoryCountUnit CountUnit { get; }

        int TotalCapacity { get; }
    }
}
