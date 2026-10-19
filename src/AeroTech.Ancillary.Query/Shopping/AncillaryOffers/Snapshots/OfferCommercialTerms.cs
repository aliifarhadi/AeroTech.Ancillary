using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferCommercialTerms(
        PurchaseStage PurchaseStage,
        CommercialDisposition Disposition,
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        ConfirmationRequirement ConfirmationRequirement,
        BookingMethod BookingMethod,
        DocumentRouting DocumentRouting,
        AncillaryDocumentType DocumentType,
        string? Rfic,
        string? Rfisc,
        IReadOnlyList<OfferCancellationRule> CancellationRules);
}
