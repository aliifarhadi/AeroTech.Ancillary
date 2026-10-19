namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferOptionChoiceDto(
        string OptionRef,
        string Value,
        string Label,
        string Availability,
        OfferApplicabilityDto Applicability,
        OfferPricePreviewDto? PricePreview,
        string? SourceVersion,
        DateTimeOffset? ValidUntilUtc);
}
