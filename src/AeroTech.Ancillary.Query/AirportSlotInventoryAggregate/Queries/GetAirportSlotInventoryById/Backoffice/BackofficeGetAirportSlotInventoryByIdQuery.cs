using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById.Backoffice
{
    public sealed record BackofficeGetAirportSlotInventoryByIdQuery(long InventoryId) : IRequest<BackofficeAirportSlotInventoryDto>;
}
