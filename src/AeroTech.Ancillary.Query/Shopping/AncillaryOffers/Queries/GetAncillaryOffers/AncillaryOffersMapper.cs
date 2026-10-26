using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public static class AncillaryOffersMapper
    {
        private const string TimeFormat = "HH:mm:ss";

        public static AncillaryOffersDto Map(AncillaryOffersSnapshot snapshot)
            => new(
                snapshot.OfferId,
                snapshot.CurrencyId,
                Family(snapshot, AncillaryProfile.Baggage, row => Baggage(row.Definition)),
                Family(snapshot, AncillaryProfile.Meal, row => Meal(row.Definition)),
                Family(snapshot, AncillaryProfile.Pet, row => Pet(row.Definition)),
                Family(snapshot, AncillaryProfile.AssistedTravel, row => AssistedTravel(row.Definition)),
                Family(snapshot, AncillaryProfile.AirportService, row => AirportService(row.Definition, row.Provision)),
                Family(snapshot, AncillaryProfile.Priority, row => Priority(row.Definition)),
                Family(snapshot, AncillaryProfile.Connectivity, row => Connectivity(row.Definition)),
                Family(snapshot, AncillaryProfile.Upgrade, row => Upgrade(row.Definition)),
                [],
                []);

        public static IEnumerable<IGrouping<string, AncillaryOfferRow>> VariantsOf(AncillaryOffersSnapshot snapshot, AncillaryProfile profile)
            => snapshot.Rows.Where(row => row.Candidate.Profile == profile).GroupBy(row => row.Candidate.VariantCode, StringComparer.Ordinal);

        public static IEnumerable<IReadOnlyList<AncillaryOfferRow>> ProductsOf(IEnumerable<AncillaryOfferRow> variant)
            => variant
                .GroupBy(row => (row.Candidate.DefinitionVersionId, row.Candidate.ProvisionId))
                .Select(product => (IReadOnlyList<AncillaryOfferRow>)product.ToList());

        public static string VariantTitle(string variantCode) => AncillaryVariant.Find(variantCode)!.Name;

        public static string ProductKey(AncillaryOfferRow row)
            => string.Create(CultureInfo.InvariantCulture, $"{row.Candidate.DefinitionVersionId}:{row.Candidate.ProvisionId}");

        public static OfferUnitDto Unit(AncillaryOfferRow row)
            => Unit(row.SelectionScope, row.Candidate.CandidateIdentity.TravellerRef, row.Candidate.CandidateIdentity.CoverageRef);

        public static OfferUnitDto Unit(SelectionScope scope, string travellerRef, string coverageRef)
            => scope switch
            {
                SelectionScope.PerOrder => new OrderUnitDto(nameof(ServiceCoverageScope.Order)),
                SelectionScope.PerTraveller => new TravellerUnitDto(travellerRef),
                SelectionScope.PerTravellerPerPortion => new TravellerPortionUnitDto(travellerRef, coverageRef),
                SelectionScope.PerTravellerPerFlight => new TravellerFlightUnitDto(travellerRef, coverageRef),
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
            };

        public static OfferCoverageDto Coverage(AncillaryOfferRow row)
            => new(row.Candidate.Scope.TravellerRefs, row.Candidate.Scope.PortionRefs, row.Candidate.Scope.FlightRefs);

        public static OfferMoneyDto UnitPrice(AncillaryOffersSnapshot snapshot, AncillaryOfferRow row) => new(snapshot.CurrencyId, row.UnitAmount);

        public static BaggageDetailsDto Baggage(AncillaryServiceDefinition definition)
        {
            var baggage = definition.Baggage!;

            return new BaggageDetailsDto(
                baggage.ChargeKind,
                baggage.AllowanceConcept,
                baggage.PackageWeightKg,
                baggage.MaxKgPerPiece,
                baggage.WeightFromExclusiveKg,
                baggage.WeightToInclusiveKg,
                baggage.MaxSize is null ? null : Dimensions(baggage.MaxSize),
                baggage.MaxLinearSumCm,
                baggage.EquipmentKind,
                baggage.ChargeCombination);
        }

        public static MealDetailsDto Meal(AncillaryServiceDefinition definition)
        {
            var meal = definition.Meal!;

            return new MealDetailsDto(meal.MealKind, meal.MealCode, meal.MenuItemRef, meal.DietaryCode, meal.CateringLeadTimeMinutes, meal.ExclusiveMealFamilyCode);
        }

        public static PetDetailsDto Pet(AncillaryServiceDefinition definition)
        {
            var pet = definition.Pet!;

            return new PetDetailsDto(
                pet.TransportMode,
                pet.AllowedAnimalTypes.Select(animal => new PetAnimalDto(animal.AnimalType, animal.OtherCode.Length == 0 ? null : animal.OtherCode)).ToList(),
                pet.MaxCombinedWeightKg,
                Dimensions(pet.CarrierDimensionsMaxCm),
                pet.MinAnimalAgeWeeks,
                Codes(pet.RequiredDocumentCodes),
                pet.AllowedHoldAnimalSizeBrackets.Count == 0
                    ? null
                    : pet.AllowedHoldAnimalSizeBrackets.Select(bracket => new PetSizeBracketDto(bracket.Code, bracket.WeightFromExclusiveKg, bracket.WeightToInclusiveKg)).ToList());
        }

        public static AssistedTravelDetailsDto AssistedTravel(AncillaryServiceDefinition definition)
        {
            var assisted = definition.AssistedTravel!;

            return new AssistedTravelDetailsDto(
                assisted.AssistanceKind,
                assisted.Wheelchair is { } wheelchair
                    ? new WheelchairDetailsDto(Codes(wheelchair.AllowedSsrCodes), wheelchair.AssistanceLevelCode, wheelchair.LeadTimeMinutes)
                    : null,
                assisted.DisabilityAssistance is { } disability
                    ? new DisabilityAssistanceDetailsDto(Codes(disability.AllowedSsrCodes), disability.RequiredCommunicationMethod)
                    : null,
                assisted.Bassinet is { } bassinet
                    ? new BassinetDetailsDto(bassinet.MaxInfantWeightKg, bassinet.MaxInfantAgeMonths, Codes(bassinet.CompatibleSeatGroups), bassinet.RequiresInfantAndGuardian)
                    : null);
        }

        public static AirportServiceDetailsDto AirportService(AncillaryServiceDefinition definition, AncillaryProvision provision)
        {
            var airport = definition.AirportService!;
            var rule = provision.AirportServiceRule;

            return new AirportServiceDetailsDto(
                airport.Kind,
                airport.AirportId,
                rule?.TerminalRef ?? airport.TerminalRef,
                rule?.FacilityId ?? airport.FacilityId,
                rule?.Direction ?? airport.Direction,
                Time(rule?.ServiceWindowStart ?? airport.ServiceWindowStart),
                Time(rule?.ServiceWindowEnd ?? airport.ServiceWindowEnd),
                airport.IanaTimeZone,
                airport.VisitDurationMinutes,
                rule?.MaxGuestsPerPrimary ?? airport.MaxGuestsPerPrimary,
                airport.RequiresSpecificAppointment,
                airport.IncludedComponentCodes.Count == 0 ? null : Codes(airport.IncludedComponentCodes));
        }

        public static PriorityDetailsDto Priority(AncillaryServiceDefinition definition)
        {
            var priority = definition.Priority!;

            return new PriorityDetailsDto(priority.Kind, priority.PriorityZoneCode, priority.PriorityGroupCode, priority.FareBenefitRef, Ids(priority.Airports));
        }

        public static ConnectivityDetailsDto Connectivity(AncillaryServiceDefinition definition)
        {
            var connectivity = definition.Connectivity!;

            return new ConnectivityDetailsDto(
                connectivity.PlanKind,
                connectivity.DurationMinutes,
                connectivity.IncludedDataMb,
                connectivity.MaxDevices,
                connectivity.DeliveryStage,
                connectivity.FulfillmentProviderRef,
                Ids(connectivity.EligibleAircraft));
        }

        public static UpgradeDetailsDto Upgrade(AncillaryServiceDefinition definition)
        {
            var upgrade = definition.Upgrade!;

            return new UpgradeDetailsDto(
                upgrade.FromCabinId,
                upgrade.ToCabinId,
                upgrade.AllowedUpgradeKind,
                upgrade.RequiresTicketExchange,
                upgrade.EligibleFareFamilies.Select(fareFamily => fareFamily.ReferenceId).ToList());
        }

        private static IReadOnlyList<AncillaryVariantGroupDto<TDetails>> Family<TDetails>(
            AncillaryOffersSnapshot snapshot,
            AncillaryProfile profile,
            Func<AncillaryOfferRow, TDetails> details)
            => VariantsOf(snapshot, profile)
                .Select(variant => new AncillaryVariantGroupDto<TDetails>(
                    variant.Key,
                    VariantTitle(variant.Key),
                    ProductsOf(variant).Select(rows => Product(snapshot, rows, details)).ToList()))
                .ToList();

        private static AncillaryProductGroupDto<TDetails> Product<TDetails>(
            AncillaryOffersSnapshot snapshot,
            IReadOnlyList<AncillaryOfferRow> rows,
            Func<AncillaryOfferRow, TDetails> details)
        {
            var first = rows[0];

            return new AncillaryProductGroupDto<TDetails>(
                ProductKey(first),
                first.Candidate.ServiceDefinitionRef,
                first.Candidate.CommercialName,
                first.Candidate.Description,
                details(first),
                rows.Select(row => UnitOffer(snapshot, row)).ToList());
        }

        private static AncillaryUnitOfferDto UnitOffer(AncillaryOffersSnapshot snapshot, AncillaryOfferRow row)
            => new(
                row.SelectionScope,
                Unit(row),
                Coverage(row),
                UnitPrice(snapshot, row),
                new OfferQuantityPolicyDto(row.QuantityPolicy.Selectable, row.QuantityPolicy.Min, row.QuantityPolicy.Max, row.QuantityPolicy.Unit),
                row.OfferItemId!,
                SelectionOptionsMapper.Options(row),
                SelectionOptionsMapper.Selected(row));

        private static OfferDimensionsDto Dimensions(DimensionsCm dimensions) => new(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);

        private static IReadOnlyList<string> Codes(IEnumerable<SpecificationCode> codes) => codes.Select(code => code.Code).ToList();

        private static IReadOnlyList<int> Ids(IEnumerable<SpecificationReference> references) => references.Select(reference => checked((int)reference.ReferenceId)).ToList();

        private static string? Time(TimeOnly? time) => time?.ToString(TimeFormat, CultureInfo.InvariantCulture);
    }
}
