using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts
{
    public sealed record AncillaryProvisionReadModelSnapshot(
        long ProvisionId,
        long ServiceDefinitionId,
        int Sequence,
        ProvisionStatus Status,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        ServiceCoverageScope CoverageScope,
        AncillaryQuantityUnit QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        ProvisionApplicationType ApplicationType,
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        int? FeeCurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        IReadOnlyList<ProvisionPriceLineReadModelSnapshot> PriceLines);

    public sealed record ProvisionPriceLineReadModelSnapshot(
        long PriceLineId,
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount);
}
