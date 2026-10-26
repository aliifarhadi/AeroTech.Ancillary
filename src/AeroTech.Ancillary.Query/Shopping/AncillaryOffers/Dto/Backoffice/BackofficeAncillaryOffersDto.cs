namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryOffersDto(
        string OfferId,
        int CurrencyId,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeBaggageDetailsDto>> Baggages,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeMealDetailsDto>> Meals,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficePetDetailsDto>> Pets,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeAssistedTravelDetailsDto>> AssistedTravel,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeAirportServiceDetailsDto>> AirportServices,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficePriorityDetailsDto>> PriorityServices,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeConnectivityDetailsDto>> Connectivity,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<BackofficeUpgradeDetailsDto>> Upgrades,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<ExtraSeatDetailsDto>> ExtraSeats,
        IReadOnlyList<BackofficeAncillaryVariantGroupDto<TravelInsuranceDetailsDto>> TravelInsurance);
}
