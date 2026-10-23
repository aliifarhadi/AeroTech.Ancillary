using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public static class BackofficeAncillaryOffersMapper
    {
        public static BackofficeAncillaryOffersDto Map(AncillaryOffersSnapshot snapshot, string surface)
            => new(
                snapshot.OfferId,
                snapshot.CurrencyId,
                Family(snapshot, surface, AncillaryProfile.Baggage, row => Baggage(AncillaryOffersMapper.Baggage(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.Meal, row => Meal(AncillaryOffersMapper.Meal(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.Pet, row => Pet(AncillaryOffersMapper.Pet(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.AssistedTravel, row => AssistedTravel(AncillaryOffersMapper.AssistedTravel(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.AirportService, row => AirportService(AncillaryOffersMapper.AirportService(row.Definition, row.Provision))),
                Family(snapshot, surface, AncillaryProfile.Priority, row => Priority(AncillaryOffersMapper.Priority(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.Connectivity, row => Connectivity(AncillaryOffersMapper.Connectivity(row.Definition))),
                Family(snapshot, surface, AncillaryProfile.Upgrade, row => Upgrade(AncillaryOffersMapper.Upgrade(row.Definition))),
                [],
                []);

        private static IReadOnlyList<BackofficeAncillaryOfferDto<TDetails>> Family<TDetails>(
            AncillaryOffersSnapshot snapshot,
            string surface,
            AncillaryProfile profile,
            Func<AncillaryOfferRow, TDetails> details)
            => AncillaryOffersMapper.RowsOf(snapshot, profile)
                .Select(row => new BackofficeAncillaryOfferDto<TDetails>(
                    row.Candidate.VariantCode,
                    row.Candidate.CommercialName,
                    row.Candidate.Description,
                    EnumValueDto.Of(row.SelectionScope),
                    AncillaryOffersMapper.Unit(row),
                    AncillaryOffersMapper.Coverage(row),
                    AncillaryOffersMapper.UnitPrice(snapshot, row),
                    new BackofficeQuantityPolicyDto(row.QuantityPolicy.Selectable, row.QuantityPolicy.Min, row.QuantityPolicy.Max, EnumValueDto.Of(row.QuantityPolicy.Unit)),
                    row.SelectionId,
                    AncillaryOffersMapper.QuantityOptions(row),
                    row.Reference,
                    AncillaryOffersMapper.OptionsEndpoint(surface, row),
                    details(row)))
                .ToList();

        private static BackofficeBaggageDetailsDto Baggage(BaggageDetailsDto baggage)
            => new(
                EnumValueDto.Of(baggage.ChargeKind),
                EnumValueDto.OfNullable(baggage.AllowanceConcept),
                baggage.PackageWeightKg,
                baggage.MaxKgPerPiece,
                baggage.WeightFromExclusiveKg,
                baggage.WeightToInclusiveKg,
                baggage.MaxSizeCm,
                baggage.MaxLinearSumCm,
                baggage.EquipmentKind,
                EnumValueDto.OfNullable(baggage.ChargeCombination));

        private static BackofficeMealDetailsDto Meal(MealDetailsDto meal)
            => new(EnumValueDto.Of(meal.MealKind), meal.MealCode, meal.MenuItemRef, meal.DietaryCode, meal.CateringLeadTimeMinutes, meal.ExclusiveMealFamilyCode);

        private static BackofficePetDetailsDto Pet(PetDetailsDto pet)
            => new(
                EnumValueDto.Of(pet.TransportMode),
                pet.AllowedAnimalTypes.Select(animal => new BackofficePetAnimalDto(EnumValueDto.Of(animal.AnimalType), animal.OtherCode)).ToList(),
                pet.MaxCombinedWeightKg,
                pet.CarrierDimensionsMaxCm,
                pet.MinAnimalAgeWeeks,
                pet.RequiredDocumentCodes,
                pet.AllowedHoldAnimalSizeBrackets);

        private static BackofficeAssistedTravelDetailsDto AssistedTravel(AssistedTravelDetailsDto assisted)
            => new(
                EnumValueDto.Of(assisted.AssistanceKind),
                assisted.Wheelchair,
                assisted.DisabilityAssistance is { } disability
                    ? new BackofficeDisabilityAssistanceDetailsDto(disability.AllowedSsrCodes, EnumValueDto.OfNullable(disability.RequiredCommunicationMethod))
                    : null,
                assisted.Bassinet);

        private static BackofficeAirportServiceDetailsDto AirportService(AirportServiceDetailsDto airport)
            => new(
                EnumValueDto.Of(airport.Kind),
                airport.AirportId,
                airport.TerminalRef,
                airport.FacilityId,
                EnumValueDto.Of(airport.Direction),
                airport.ServiceWindowStart,
                airport.ServiceWindowEnd,
                airport.IanaTimeZone,
                airport.VisitDurationMinutes,
                airport.MaxGuestsPerPrimary,
                airport.RequiresSpecificAppointment,
                airport.IncludedComponentCodes);

        private static BackofficePriorityDetailsDto Priority(PriorityDetailsDto priority)
            => new(EnumValueDto.Of(priority.Kind), priority.PriorityZoneCode, priority.PriorityGroupCode, priority.FareBenefitRef, priority.AirportIds);

        private static BackofficeConnectivityDetailsDto Connectivity(ConnectivityDetailsDto connectivity)
            => new(
                EnumValueDto.Of(connectivity.PlanKind),
                connectivity.DurationMinutes,
                connectivity.IncludedDataMb,
                connectivity.MaxDevices,
                EnumValueDto.Of(connectivity.DeliveryStage),
                connectivity.FulfillmentProviderRef,
                connectivity.EligibleAircraftIds);

        private static BackofficeUpgradeDetailsDto Upgrade(UpgradeDetailsDto upgrade)
            => new(upgrade.FromCabinId, upgrade.ToCabinId, EnumValueDto.Of(upgrade.AllowedUpgradeKind), upgrade.RequiresTicketExchange, upgrade.EligibleFareFamilyIds);
    }
}
