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
        public static BackofficeAncillaryOffersDto Map(AncillaryOffersSnapshot snapshot)
            => new(
                snapshot.OfferId,
                snapshot.CurrencyId,
                Family(snapshot, AncillaryProfile.Baggage, row => Baggage(AncillaryOffersMapper.Baggage(row.Definition))),
                Family(snapshot, AncillaryProfile.Meal, row => Meal(AncillaryOffersMapper.Meal(row.Definition))),
                Family(snapshot, AncillaryProfile.Pet, row => Pet(AncillaryOffersMapper.Pet(row.Definition))),
                Family(snapshot, AncillaryProfile.AssistedTravel, row => AssistedTravel(AncillaryOffersMapper.AssistedTravel(row.Definition))),
                Family(snapshot, AncillaryProfile.AirportService, row => AirportService(AncillaryOffersMapper.AirportService(row.Definition, row.Provision))),
                Family(snapshot, AncillaryProfile.Priority, row => Priority(AncillaryOffersMapper.Priority(row.Definition))),
                Family(snapshot, AncillaryProfile.Connectivity, row => Connectivity(AncillaryOffersMapper.Connectivity(row.Definition))),
                Family(snapshot, AncillaryProfile.Upgrade, row => Upgrade(AncillaryOffersMapper.Upgrade(row.Definition))),
                [],
                []);

        private static IReadOnlyList<BackofficeAncillaryVariantGroupDto<TDetails>> Family<TDetails>(
            AncillaryOffersSnapshot snapshot,
            AncillaryProfile profile,
            Func<AncillaryOfferRow, TDetails> details)
            => AncillaryOffersMapper.VariantsOf(snapshot, profile)
                .Select(variant => new BackofficeAncillaryVariantGroupDto<TDetails>(
                    variant.Key,
                    AncillaryOffersMapper.VariantTitle(variant.Key),
                    AncillaryOffersMapper.ProductsOf(variant).Select(rows => Product(snapshot, rows, details)).ToList()))
                .ToList();

        private static BackofficeAncillaryProductGroupDto<TDetails> Product<TDetails>(
            AncillaryOffersSnapshot snapshot,
            IReadOnlyList<AncillaryOfferRow> rows,
            Func<AncillaryOfferRow, TDetails> details)
        {
            var first = rows[0];

            return new BackofficeAncillaryProductGroupDto<TDetails>(
                AncillaryOffersMapper.ProductKey(first),
                first.Candidate.ServiceDefinitionRef,
                first.Candidate.CommercialName,
                first.Candidate.Description,
                details(first),
                rows.Select(row => UnitOffer(snapshot, row)).ToList());
        }

        private static BackofficeAncillaryUnitOfferDto UnitOffer(AncillaryOffersSnapshot snapshot, AncillaryOfferRow row)
            => new(
                EnumValueDto.Of(row.SelectionScope),
                AncillaryOffersMapper.Unit(row),
                AncillaryOffersMapper.Coverage(row),
                AncillaryOffersMapper.UnitPrice(snapshot, row),
                new BackofficeQuantityPolicyDto(row.QuantityPolicy.Selectable, row.QuantityPolicy.Min, row.QuantityPolicy.Max, EnumValueDto.Of(row.QuantityPolicy.Unit)),
                row.OfferItemId!,
                SelectionOptionsMapper.Options(row),
                SelectionOptionsMapper.Selected(row));

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
