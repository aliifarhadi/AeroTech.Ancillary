namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceGroupedResolvedSelectionDto(
        string SelectionId,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        ServiceResolvedServiceDto? Service);
}
