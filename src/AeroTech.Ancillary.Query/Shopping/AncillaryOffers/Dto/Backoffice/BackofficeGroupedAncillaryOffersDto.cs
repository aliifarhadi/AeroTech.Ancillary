namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedAncillaryOffersDto(
        BackofficeGroupedOffersMetaDto Meta,
        IReadOnlyList<BackofficeGroupedSeatLinkDto> Seats,
        IReadOnlyList<BackofficeGroupedBaggageOfferDto> Baggages,
        IReadOnlyList<BackofficeGroupedMealOfferDto> Meals,
        IReadOnlyList<BackofficeGroupedPetOfferDto> Pets,
        IReadOnlyList<BackofficeGroupedAssistedTravelOfferDto> AssistedTravel,
        IReadOnlyList<BackofficeGroupedAirportServiceOfferDto> AirportServices,
        IReadOnlyList<BackofficeGroupedPriorityOfferDto> PriorityServices,
        IReadOnlyList<BackofficeGroupedConnectivityOfferDto> Connectivity,
        IReadOnlyList<BackofficeGroupedUpgradeOfferDto> Upgrades,
        IReadOnlyList<BackofficeGroupedExtraSeatOfferDto> ExtraSeats);
}
