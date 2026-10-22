namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedAncillaryOffersDto(
        OtaPanelGroupedOffersMetaDto Meta,
        IReadOnlyList<OtaPanelGroupedSeatLinkDto> Seats,
        IReadOnlyList<OtaPanelGroupedBaggageOfferDto> Baggages,
        IReadOnlyList<OtaPanelGroupedMealOfferDto> Meals,
        IReadOnlyList<OtaPanelGroupedPetOfferDto> Pets,
        IReadOnlyList<OtaPanelGroupedAssistedTravelOfferDto> AssistedTravel,
        IReadOnlyList<OtaPanelGroupedAirportServiceOfferDto> AirportServices,
        IReadOnlyList<OtaPanelGroupedPriorityOfferDto> PriorityServices,
        IReadOnlyList<OtaPanelGroupedConnectivityOfferDto> Connectivity,
        IReadOnlyList<OtaPanelGroupedUpgradeOfferDto> Upgrades,
        IReadOnlyList<OtaPanelGroupedExtraSeatOfferDto> ExtraSeats);
}
