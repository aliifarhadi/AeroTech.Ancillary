using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated
{
    public interface IAncillaryProductsPaginatedQuery
    {
        int? OwnerAirlineId { get; }

        string? ProductRef { get; }

        AncillaryProductType? Type { get; }

        AncillaryProductStatus? Status { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
