namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiOptionChoiceDto(
        string OptionRef,
        string Value,
        string Label,
        string Availability,
        ApiOptionApplicabilityDto Applicability,
        ApiOptionPricePreviewDto? PricePreview,
        DateTimeOffset? ValidUntilUtc);
}
