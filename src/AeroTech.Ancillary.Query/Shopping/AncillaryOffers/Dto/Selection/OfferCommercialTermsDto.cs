namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferCommercialTermsDto(
        string PurchaseStage,
        string Disposition,
        string ReissueRefund,
        string? FormOfRefund,
        string ConfirmationRequirement,
        string BookingMethod,
        string DocumentRouting,
        string DocumentType,
        string? Rfic,
        string? Rfisc,
        IReadOnlyList<OfferCancellationRuleDto> CancellationRules);
}
