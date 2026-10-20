namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferInlineOptionGroupDto(
        string FieldName,
        string SourceState,
        IReadOnlyList<OfferOptionChoiceDto> Choices,
        int TotalChoices,
        bool InlineChoicesIncluded);
}
