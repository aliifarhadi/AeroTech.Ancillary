namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferSelectedOptionDto(
        string FieldName,
        string Value,
        string? OptionRef);
}
