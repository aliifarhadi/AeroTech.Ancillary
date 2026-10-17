using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        private static readonly IReadOnlyDictionary<PricingUnit, AncillaryQuantityUnit> QuantityUnits =
            new Dictionary<PricingUnit, AncillaryQuantityUnit>
            {
                [PricingUnit.PerPassenger] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerRoom] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerItem] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerVehicle] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerSeat] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerPiece] = AncillaryQuantityUnit.Piece,
                [PricingUnit.PerKilogram] = AncillaryQuantityUnit.Kilogram
            };

        private static readonly TimeUnit[] ActivatableAdvancePurchaseUnits =
        [
            TimeUnit.Minutes,
            TimeUnit.Hours,
            TimeUnit.Days,
            TimeUnit.Months
        ];

        private bool IsFlightBound
            => IsFlightBoundBy(
                ApplicationType,
                FlightApplication is not null,
                FareApplication is not null,
                AdvancePurchase is { SameTimeAsTicketed: true },
                Geography is { OriginAirports.Count: > 0 }
                    or { DestinationAirports.Count: > 0 }
                    or { ViaAirports.Count: > 0 }
                    or { RoutePairs.Count: > 0 });

        private bool NeedsServiceOccurrence
            => IsFlightBound
               || TravelDate is not null
               || DayTimeApplication is not null
               || AdvancePurchase is not null
               || PassengerEligibility is { AgeBands.Count: > 0 };

        public void EnsurePublishable(AncillaryServiceDefinition definition)
        {
            Require(definition.Id == ServiceDefinitionId, nameof(ServiceDefinitionId));

            if (!definition.IsClassified)
                throw ExceptionFactory.ProvisionDefinitionNotClassified();

            var pricingUnit = definition.PricingUnit ?? throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            EnsureQuantityUnitOf(pricingUnit, Quantity.Unit);

            if (NeedsServiceOccurrence && definition.ServiceDateBasis is null)
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisNotAssigned();

            EnsureRuleContextOf(definition.ServiceDateBasis, IsFlightBound);
            EnsureAdvancePurchaseUnitOf(AdvancePurchase?.Unit);

            if (TravelDate is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(TravelDate));

            if (DayTimeApplication is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(DayTimeApplication));
        }

        private void EnsureProfileConformance(AncillaryServiceDefinition definition, string? recordedQuoteProviderKey)
        {
            var variant = definition.Variant!;
            var facts = StoredFacts();

            EnsureApplicationTypeOf(variant, facts.ApplicationType);

            if (PriceOrigin == PriceOrigin.LegacyUnspecified)
                throw ExceptionFactory.ProvisionPriceOriginMismatch(Outcome.Disposition, PriceOrigin);

            EnsurePurchaseStageOf(definition, variant, facts.PurchaseStage);

            if (PriceOrigin == PriceOrigin.ExternalQuote && (recordedQuoteProviderKey is null || !string.Equals(recordedQuoteProviderKey, QuoteProviderKey, StringComparison.Ordinal)))
                throw ExceptionFactory.ProvisionQuoteAuthorityRequired(QuoteProviderKey);

            EnsureOutcomeOf(definition, variant, facts);
            EnsureRulesOf(definition, variant, facts);
        }

        private static void EnsureAuthoringConformance(AncillaryServiceDefinition definition, AuthoringFacts facts)
        {
            if (definition.Variant is not { } variant)
                return;

            EnsureApplicationTypeOf(variant, facts.ApplicationType);

            if (definition.PricingUnit is { } pricingUnit)
                EnsureQuantityUnitOf(pricingUnit, facts.QuantityUnit);

            EnsurePurchaseStageOf(definition, variant, facts.PurchaseStage);
            EnsureOutcomeOf(definition, variant, facts);
            EnsureRuleContextOf(definition.ServiceDateBasis, facts.IsFlightBound);
            EnsureAdvancePurchaseUnitOf(facts.AdvancePurchaseUnit);
            EnsureRulesOf(definition, variant, facts);
        }

        private static AuthoringFacts CandidateFacts(
            ProvisionApplicationType applicationType,
            PurchaseStage purchaseStage,
            PriceOrigin priceOrigin,
            QuantityRule quantity,
            CommercialOutcome outcome,
            ProvisionRulesArgs rules)
            => new(
                applicationType,
                purchaseStage,
                outcome.Disposition,
                priceOrigin,
                quantity.Unit,
                quantity.MaxQuantity,
                IsFlightBoundBy(
                    applicationType,
                    rules.FlightApplication is { IsEmpty: false },
                    rules.FareApplication is { IsEmpty: false },
                    rules.AdvancePurchase is { SameTimeAsTicketed: true },
                    rules.Geography is { OriginAirportIds.Count: > 0 }
                        or { DestinationAirportIds.Count: > 0 }
                        or { ViaAirportIds.Count: > 0 }
                        or { RoutePairs.Count: > 0 }),
                rules.AdvancePurchase?.Unit,
                rules.BaggageApplication?.ChargeKind,
                rules.BaggageApplication?.AllowanceConcept,
                rules.FareApplication?.CabinClassIds ?? [],
                rules.PetRule,
                rules.AssistedTravelRule,
                rules.AirportServiceRule);

        private AuthoringFacts StoredFacts()
            => new(
                ApplicationType,
                PurchaseStage,
                Outcome.Disposition,
                PriceOrigin,
                Quantity.Unit,
                Quantity.MaxQuantity,
                IsFlightBound,
                AdvancePurchase?.Unit,
                BaggageApplication?.ChargeKind,
                BaggageApplication?.AllowanceConcept,
                FareApplication?.CabinClasses.Select(cabin => cabin.CabinClassId).ToList() ?? [],
                PetRule is { } petRule
                    ? new ProvisionPetRuleArgs(petRule.CountryExceptionCode, petRule.MinAnimalAgeWeeksOverride, petRule.MaxCombinedKgOverride, petRule.AcceptanceMode)
                    : null,
                AssistedTravelRule is { } assistedRule
                    ? new ProvisionAssistedTravelRuleArgs(assistedRule.MinimumLeadTimeMinutes, assistedRule.ConnectionPolicy, assistedRule.MedicalApprovalRequired)
                    : null,
                AirportServiceRule is { } airportRule
                    ? new ProvisionAirportServiceRuleArgs(
                        airportRule.TerminalRef,
                        airportRule.Direction,
                        airportRule.ServiceWindowStart,
                        airportRule.ServiceWindowEnd,
                        airportRule.FacilityId,
                        airportRule.MaxGuestsPerPrimary)
                    : null);

        private static bool IsFlightBoundBy(
            ProvisionApplicationType applicationType,
            bool flightApplication,
            bool fareApplication,
            bool sameTimeAsTicketed,
            bool routedGeography)
            => applicationType != ProvisionApplicationType.Standard
               || flightApplication
               || fareApplication
               || sameTimeAsTicketed
               || routedGeography;

        private static void EnsureQuantityUnitOf(PricingUnit pricingUnit, AncillaryQuantityUnit quantityUnit)
        {
            if (QuantityUnits[pricingUnit] != quantityUnit)
                throw ExceptionFactory.ProvisionQuantityUnitIncompatible(pricingUnit, quantityUnit);
        }

        private static void EnsureRuleContextOf(ServiceDateBasis? serviceDateBasis, bool flightBound)
        {
            if (flightBound && serviceDateBasis is { } basis && basis != ServiceDateBasis.FlightDeparture)
                throw ExceptionFactory.ProvisionRuleContextNotSupported(basis);
        }

        private static void EnsureAdvancePurchaseUnitOf(TimeUnit? unit)
        {
            if (unit is { } advanceUnit && !ActivatableAdvancePurchaseUnits.Contains(advanceUnit))
                throw ExceptionFactory.ProvisionAdvancePurchaseUnitNotSupported(advanceUnit);
        }

        private static void EnsureApplicationTypeOf(AncillaryVariant variant, ProvisionApplicationType applicationType)
        {
            if (applicationType != variant.ApplicationType)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(ApplicationType), variant.Code);
        }

        private static void EnsurePurchaseStageOf(AncillaryServiceDefinition definition, AncillaryVariant variant, PurchaseStage purchaseStage)
        {
            if (!AllowedPurchaseStages(definition).Contains(purchaseStage))
                throw ExceptionFactory.ProvisionPurchaseStageNotAllowed(purchaseStage, variant.Code);
        }

        private static void EnsureOutcomeOf(AncillaryServiceDefinition definition, AncillaryVariant variant, AuthoringFacts facts)
        {
            if (!AllowedDispositions(definition).Contains(facts.Disposition))
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(Outcome), variant.Code);

            if (!AllowedPriceOrigins(definition).Contains(facts.PriceOrigin))
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(PriceOrigin), variant.Code);

            if (MaxQuantityOf(definition) is { } maxQuantity && facts.MaxQuantity > maxQuantity)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(Quantity), variant.Code);
        }

        private static void EnsureRulesOf(AncillaryServiceDefinition definition, AncillaryVariant variant, AuthoringFacts facts)
        {
            void Refuse(bool refused, string field)
            {
                if (refused)
                    throw ExceptionFactory.ProvisionProfileRuleNotAllowed(field, variant.Code);
            }

            Refuse(facts.PetRule is not null && definition.Pet is null, nameof(PetRule));
            Refuse(facts.AssistedTravelRule is not null && definition.AssistedTravel is null, nameof(AssistedTravelRule));
            Refuse(facts.AirportServiceRule is not null && definition.AirportService is null, nameof(AirportServiceRule));

            if (definition.Baggage is { } baggageSpecification)
            {
                Refuse(facts.ChargeKind is { } chargeKind && chargeKind != baggageSpecification.ChargeKind, nameof(BaggageApplication));
                Refuse(
                    facts.AllowanceConcept is { } concept && baggageSpecification.AllowanceConcept is { } specified && concept != specified,
                    nameof(BaggageApplication));
            }

            if (definition.Upgrade is { } upgrade && facts.CabinClassIds.Count > 0)
                Refuse(facts.CabinClassIds.All(cabinClassId => cabinClassId != upgrade.FromCabinId), nameof(FareApplication));

            if (facts.PetRule is { } petRule && definition.Pet is { } pet)
            {
                Refuse(petRule.AcceptanceMode != ConfirmationRequirement.SubjectToConfirmation, nameof(PetRule));
                Refuse(petRule.MaxCombinedKgOverride > pet.MaxCombinedWeightKg, nameof(PetRule));
                Refuse(petRule.MinAnimalAgeWeeksOverride < (pet.MinAnimalAgeWeeks ?? 0), nameof(PetRule));
            }

            if (facts.AssistedTravelRule is { } assistedRule && definition.AssistedTravel is { } assisted)
            {
                Refuse(assistedRule.ConnectionPolicy is not null && assisted.UnaccompaniedMinor is null, nameof(AssistedTravelRule));
                Refuse(
                    assistedRule.ConnectionPolicy == MinorConnectionPolicy.ApprovedConnections
                    && assisted.UnaccompaniedMinor is { ConnectionPolicy: MinorConnectionPolicy.DirectOnly },
                    nameof(AssistedTravelRule));
                Refuse(assistedRule.MedicalApprovalRequired is not null && assisted.MedicalEquipment is null, nameof(AssistedTravelRule));
                Refuse(
                    assistedRule.MedicalApprovalRequired == false && assisted.MedicalEquipment is { RequiresMedicalApproval: true },
                    nameof(AssistedTravelRule));
                Refuse(assistedRule.MinimumLeadTimeMinutes < (assisted.Wheelchair?.LeadTimeMinutes ?? 0), nameof(AssistedTravelRule));
            }

            if (facts.AirportServiceRule is { } airportRule && definition.AirportService is { } airport)
            {
                Refuse(airportRule.MaxGuestsPerPrimary > (airport.MaxGuestsPerPrimary ?? 0), nameof(AirportServiceRule));
                Refuse(airportRule.FacilityId is not null && airport.FacilityId is not null && airportRule.FacilityId != airport.FacilityId, nameof(AirportServiceRule));
                Refuse(airportRule.TerminalRef is not null && airport.TerminalRef is not null && airportRule.TerminalRef != airport.TerminalRef, nameof(AirportServiceRule));
                Refuse(
                    airportRule.Direction is { } direction && airport.Direction != AirportServiceDirection.Any && direction != airport.Direction,
                    nameof(AirportServiceRule));
            }
        }

        private void EnsureDescriptorsAreStated()
        {
            if (PurchaseStage == PurchaseStage.LegacyUnspecified)
                throw ExceptionFactory.ProvisionPurchaseStageRequired();
        }

        private sealed record AuthoringFacts(
            ProvisionApplicationType ApplicationType,
            PurchaseStage PurchaseStage,
            CommercialDisposition Disposition,
            PriceOrigin PriceOrigin,
            AncillaryQuantityUnit QuantityUnit,
            int MaxQuantity,
            bool IsFlightBound,
            TimeUnit? AdvancePurchaseUnit,
            BaggageChargeKind? ChargeKind,
            BaggageAllowanceConcept? AllowanceConcept,
            IReadOnlyCollection<int> CabinClassIds,
            ProvisionPetRuleArgs? PetRule,
            ProvisionAssistedTravelRuleArgs? AssistedTravelRule,
            ProvisionAirportServiceRuleArgs? AirportServiceRule);
    }
}
