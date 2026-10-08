using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated
{
    public interface IAncillaryPricingsPaginatedQuery
    {
        long? AncillaryProvisionId { get; }

        PricingStatus? Status { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
