using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed record ProvisionQuantityInput(
        AncillaryQuantityUnit Unit,
        int MinQuantity,
        int MaxQuantity);

    public sealed record ProvisionApplicationInput(ProvisionApplicationType Type);

    public sealed record ProvisionOutcomeInput(
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired);

    public sealed record ProvisionFeeInput(
        FeeApplicationUnit ApplicationUnit,
        int CurrencyId,
        IReadOnlyList<ProvisionPriceLineInput> PriceLines);

    public sealed record ProvisionPriceLineInput(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount);

    public sealed record ProvisionSettlementInput(
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement);

    public sealed record ProvisionAvailabilityInput(bool MustCheckAvailability);

    public sealed record ProvisionFulfillmentInput(string FulfillmentProviderKey);
}
