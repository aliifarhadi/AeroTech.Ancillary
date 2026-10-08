using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById
{
    public interface IGetAncillaryPricingByIdService
    {
        Task<BackofficePricingDto> ExecuteAsync(long pricingId, CancellationToken cancellationToken = default);
    }
}
