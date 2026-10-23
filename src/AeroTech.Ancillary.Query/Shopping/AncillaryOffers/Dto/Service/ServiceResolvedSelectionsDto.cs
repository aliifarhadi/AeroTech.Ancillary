namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedSelectionsDto(
        IReadOnlyList<ServiceResolvedSelectionDto<BaggageDetailsDto>> Baggages,
        IReadOnlyList<ServiceResolvedSelectionDto<MealDetailsDto>> Meals,
        IReadOnlyList<ServiceResolvedSelectionDto<PetDetailsDto>> Pets,
        IReadOnlyList<ServiceResolvedSelectionDto<AssistedTravelDetailsDto>> AssistedTravel,
        IReadOnlyList<ServiceResolvedSelectionDto<AirportServiceDetailsDto>> AirportServices,
        IReadOnlyList<ServiceResolvedSelectionDto<PriorityDetailsDto>> PriorityServices,
        IReadOnlyList<ServiceResolvedSelectionDto<ConnectivityDetailsDto>> Connectivity,
        IReadOnlyList<ServiceResolvedSelectionDto<UpgradeDetailsDto>> Upgrades,
        IReadOnlyList<ServiceResolvedSelectionDto<ExtraSeatDetailsDto>> ExtraSeats,
        IReadOnlyList<ServiceResolvedSelectionDto<TravelInsuranceDetailsDto>> TravelInsurance);
}
