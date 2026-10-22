namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedOptionGroupDto(string FieldName, string SourceState, IReadOnlyList<OtaPanelGroupedOptionChoiceDto> Choices);
}
