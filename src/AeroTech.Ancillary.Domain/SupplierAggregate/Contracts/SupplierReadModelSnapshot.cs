using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SupplierAggregate.Contracts
{
    public sealed record SupplierReadModelSnapshot(
        long SupplierId,
        int OwnerAirlineId,
        string Name,
        SupplierFulfillmentKind FulfillmentKind,
        string? FulfillmentProviderKey,
        SupplierStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? RetiredAt);
}
