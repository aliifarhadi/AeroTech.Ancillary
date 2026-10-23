namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryOffersDto(
        string OfferId,
        int CurrencyId,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeBaggageDetailsDto>> Baggages,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeMealDetailsDto>> Meals,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficePetDetailsDto>> Pets,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeAssistedTravelDetailsDto>> AssistedTravel,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeAirportServiceDetailsDto>> AirportServices,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficePriorityDetailsDto>> PriorityServices,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeConnectivityDetailsDto>> Connectivity,
        IReadOnlyList<BackofficeAncillaryOfferDto<BackofficeUpgradeDetailsDto>> Upgrades,
        IReadOnlyList<BackofficeAncillaryOfferDto<ExtraSeatDetailsDto>> ExtraSeats,
        IReadOnlyList<BackofficeAncillaryOfferDto<TravelInsuranceDetailsDto>> TravelInsurance);
}
