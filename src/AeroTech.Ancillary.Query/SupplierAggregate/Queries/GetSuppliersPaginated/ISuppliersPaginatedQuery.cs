using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated
{
    public interface ISuppliersPaginatedQuery
    {
        int? OwnerAirlineId { get; }

        SupplierFulfillmentKind? FulfillmentKind { get; }

        SupplierStatus? Status { get; }

        string? Search { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
