namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedSelectionDto(
        string PricedSelectionRef,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        ServiceResolvedServiceDto? Service,
        ServiceIncludedSeatDto? IncludedSeat);
}
