namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelAncillaryOffersDto(
        string OfferId,
        int CurrencyId,
        string? CurrencyCode,
        IReadOnlyList<OtaPanelTravellerGroupDto> TravellerGroups,
        int BlockedCount,
        IReadOnlyList<OtaPanelRateOfExchangeDto> RatesOfExchange,
        IReadOnlyList<OtaPanelServiceDto> OrderScopedServices);
}
