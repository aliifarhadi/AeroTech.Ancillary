using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated
{
    public interface IAncillaryProvisionsPaginatedQuery
    {
        long ServiceDefinitionId { get; }

        long? SupplierId { get; }

        ProvisionStatus? Status { get; }

        ServiceCoverageScope? CoverageScope { get; }

        int? Sequence { get; }

        DateTimeOffset? SalesDate { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
