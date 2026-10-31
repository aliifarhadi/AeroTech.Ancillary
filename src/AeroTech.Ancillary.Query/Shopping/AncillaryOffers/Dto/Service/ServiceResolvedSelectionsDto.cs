using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedSelectionsDto(
        long? OrderId,
        int? OrderCommercialVersion,
        ShoppingSourceKind SourceKind,
        string SourceReference,
        DateTimeOffset ExpiresAtUtc,
        int SelectedLineCount,
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
