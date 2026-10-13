namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated
{
    public interface IProvisionPricingsPaginatedQuery
    {
        long AncillaryProvisionId { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
