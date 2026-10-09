using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById.Backoffice
{
    public sealed record BackofficeGetFlightWeightInventoryByIdQuery(long InventoryId) : IRequest<BackofficeFlightWeightInventoryDto>;
}
