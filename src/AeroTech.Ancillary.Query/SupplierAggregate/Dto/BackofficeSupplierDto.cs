using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Dto
{
    public sealed record BackofficeSupplierDto(
        long Id,
        int OwnerAirlineId,
        string Name,
        EnumValueDto FulfillmentKind,
        string? FulfillmentProviderKey,
        EnumValueDto Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? RetiredAt);
}
