using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeProvisionDto(
        long Id,
        long ServiceDefinitionId,
        int Sequence,
        EnumValueDto Status,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        EnumValueDto CoverageScope,
        EnumValueDto QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        EnumValueDto ApplicationType,
        EnumValueDto Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        int? FeeCurrencyId,
        EnumValueDto? FeeApplicationUnit,
        EnumValueDto ReissueRefund,
        EnumValueDto? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        IReadOnlyList<BackofficeProvisionPriceLineDto> PriceLines);

    public sealed record BackofficeProvisionPriceLineDto(
        long Id,
        EnumValueDto Category,
        string? Code,
        string? Name,
        decimal UnitAmount);
}
