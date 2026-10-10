using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface IActiveAncillaryPricingReader
    {
        Task<IReadOnlyList<AncillaryPricing>> ListActiveAsync(IReadOnlyCollection<long> ancillaryProvisionIds, CancellationToken cancellationToken = default);
    }
}
