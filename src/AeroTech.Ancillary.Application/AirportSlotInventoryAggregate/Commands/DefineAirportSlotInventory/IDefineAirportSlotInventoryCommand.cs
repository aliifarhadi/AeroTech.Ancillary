namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    public interface IDefineAirportSlotInventoryCommand
    {
        int OwnerAirlineId { get; }

        int AirportId { get; }

        long FacilityId { get; }

        DateTimeOffset StartUtc { get; }

        DateTimeOffset EndUtc { get; }

        int CapacityPersons { get; }
    }
}
