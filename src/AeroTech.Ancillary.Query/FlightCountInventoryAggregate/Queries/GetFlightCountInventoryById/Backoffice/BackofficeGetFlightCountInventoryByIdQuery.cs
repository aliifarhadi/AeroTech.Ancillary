using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById.Backoffice
{
    public sealed record BackofficeGetFlightCountInventoryByIdQuery(long InventoryId) : IRequest<BackofficeFlightCountInventoryDto>;
}
