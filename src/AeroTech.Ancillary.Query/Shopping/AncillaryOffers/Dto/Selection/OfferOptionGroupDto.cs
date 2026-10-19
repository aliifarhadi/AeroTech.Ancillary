namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferOptionGroupDto(
        string FieldName,
        string SourceState,
        IReadOnlyList<OfferOptionChoiceDto> Choices);
}
