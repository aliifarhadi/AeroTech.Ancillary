using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById.Service
{
    public sealed record ServiceGetAncillaryHoldByIdQuery(long HoldId) : IRequest<AncillaryHoldDto>;
}
