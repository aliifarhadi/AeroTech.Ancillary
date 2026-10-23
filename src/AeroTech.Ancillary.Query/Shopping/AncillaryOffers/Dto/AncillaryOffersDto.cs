namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AncillaryOffersDto(
        string OfferId,
        int CurrencyId,
        IReadOnlyList<AncillaryOfferDto<BaggageDetailsDto>> Baggages,
        IReadOnlyList<AncillaryOfferDto<MealDetailsDto>> Meals,
        IReadOnlyList<AncillaryOfferDto<PetDetailsDto>> Pets,
        IReadOnlyList<AncillaryOfferDto<AssistedTravelDetailsDto>> AssistedTravel,
        IReadOnlyList<AncillaryOfferDto<AirportServiceDetailsDto>> AirportServices,
        IReadOnlyList<AncillaryOfferDto<PriorityDetailsDto>> PriorityServices,
        IReadOnlyList<AncillaryOfferDto<ConnectivityDetailsDto>> Connectivity,
        IReadOnlyList<AncillaryOfferDto<UpgradeDetailsDto>> Upgrades,
        IReadOnlyList<AncillaryOfferDto<ExtraSeatDetailsDto>> ExtraSeats,
        IReadOnlyList<AncillaryOfferDto<TravelInsuranceDetailsDto>> TravelInsurance);
}
