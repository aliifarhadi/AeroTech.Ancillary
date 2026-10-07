using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById
{
    public interface IGetAncillaryHoldByIdService
    {
        Task<AncillaryHoldDto> ExecuteAsync(long holdId, CancellationToken cancellationToken = default);
    }
}
