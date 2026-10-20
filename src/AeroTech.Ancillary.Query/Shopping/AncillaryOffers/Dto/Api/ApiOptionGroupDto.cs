namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiOptionGroupDto(
        string FieldName,
        string SourceState,
        IReadOnlyList<ApiOptionChoiceDto> Choices,
        int TotalChoices,
        bool InlineChoicesIncluded);
}
