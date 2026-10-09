using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory.Backoffice
{
    public sealed record BackofficeDefineAirportSlotInventoryCommand(
        int OwnerAirlineId,
        int AirportId,
        long FacilityId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        int CapacityPersons) : IRequest<AirportSlotInventoryResult>, IDefineAirportSlotInventoryCommand;
}
