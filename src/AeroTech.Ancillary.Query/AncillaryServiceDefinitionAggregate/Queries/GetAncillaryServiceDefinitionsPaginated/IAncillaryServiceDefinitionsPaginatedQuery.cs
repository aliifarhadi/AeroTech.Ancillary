using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated
{
    public interface IAncillaryServiceDefinitionsPaginatedQuery
    {
        int? OwnerAirlineId { get; }

        long? SupplierId { get; }

        string? ServiceDefinitionRef { get; }

        string? ServiceSubCode { get; }

        string? ServiceTypeCode { get; }

        string? GroupCode { get; }

        ServiceDefinitionStatus? Status { get; }

        AncillaryProfile? Profile { get; }

        string? VariantCode { get; }

        string? Search { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
