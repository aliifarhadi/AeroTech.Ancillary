using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed record AncillaryOfferFamilies(
        IReadOnlyList<AncillaryVariantGroupDto<BaggageDetailsDto>> Baggages,
        IReadOnlyList<AncillaryVariantGroupDto<MealDetailsDto>> Meals,
        IReadOnlyList<AncillaryVariantGroupDto<PetDetailsDto>> Pets,
        IReadOnlyList<AncillaryVariantGroupDto<AssistedTravelDetailsDto>> AssistedTravel,
        IReadOnlyList<AncillaryVariantGroupDto<AirportServiceDetailsDto>> AirportServices,
        IReadOnlyList<AncillaryVariantGroupDto<PriorityDetailsDto>> PriorityServices,
        IReadOnlyList<AncillaryVariantGroupDto<ConnectivityDetailsDto>> Connectivity,
        IReadOnlyList<AncillaryVariantGroupDto<UpgradeDetailsDto>> Upgrades,
        IReadOnlyList<AncillaryVariantGroupDto<ExtraSeatDetailsDto>> ExtraSeats,
        IReadOnlyList<AncillaryVariantGroupDto<TravelInsuranceDetailsDto>> TravelInsurance);
}
