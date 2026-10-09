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
            => ApplicationType != ProvisionApplicationType.Standard
               || FlightApplication is not null
               || FareApplication is not null
               || AdvancePurchase is { SameTimeAsTicketed: true }
               || Geography is { OriginAirports.Count: > 0 }
                   or { DestinationAirports.Count: > 0 }
                   or { ViaAirports.Count: > 0 }
                   or { RoutePairs.Count: > 0 };

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

            if (QuantityUnits[pricingUnit] != Quantity.Unit)
                throw ExceptionFactory.ProvisionQuantityUnitIncompatible(pricingUnit, Quantity.Unit);

            if (NeedsServiceOccurrence && definition.ServiceDateBasis is null)
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisNotAssigned();

            if (IsFlightBound && definition.ServiceDateBasis != ServiceDateBasis.FlightDeparture)
                throw ExceptionFactory.ProvisionRuleContextNotSupported(definition.ServiceDateBasis);

            if (AdvancePurchase is not null && !ActivatableAdvancePurchaseUnits.Contains(AdvancePurchase.Unit))
                throw ExceptionFactory.ProvisionAdvancePurchaseUnitNotSupported(AdvancePurchase.Unit);

            if (TravelDate is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(TravelDate));

            if (DayTimeApplication is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(DayTimeApplication));
        }

        private void EnsureProfileConformance(AncillaryServiceDefinition definition, string? recordedQuoteProviderKey)
        {
            var variant = definition.Variant!;

            if (ApplicationType != variant.ApplicationType)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(ApplicationType), variant.Code);

            if (PriceOrigin == PriceOrigin.LegacyUnspecified)
                throw ExceptionFactory.ProvisionPriceOriginMismatch(Outcome.Disposition, PriceOrigin);

            if (SalesRestrictions is not { PointsOfSale.Count: 1 })
                throw ExceptionFactory.ProvisionSinglePointOfSaleRequired();

            var onBoardOnly = definition.Connectivity is { DeliveryStage: PurchaseStage.OnBoard };

            if ((PurchaseStage == PurchaseStage.OnBoard) != onBoardOnly)
                throw ExceptionFactory.ProvisionPurchaseStageNotAllowed(PurchaseStage, variant.Code);

            if (PriceOrigin == PriceOrigin.ExternalQuote && (recordedQuoteProviderKey is null || !string.Equals(recordedQuoteProviderKey, QuoteProviderKey, StringComparison.Ordinal)))
                throw ExceptionFactory.ProvisionQuoteAuthorityRequired(QuoteProviderKey);

            EnsureOutcomeOf(definition, variant);
            EnsureRulesOf(definition, variant);
        }

        private void EnsureOutcomeOf(AncillaryServiceDefinition definition, AncillaryVariant variant)
        {
            var paid = Outcome.Disposition == CommercialDisposition.Paid;

            if (variant.Code is AncillaryVariant.FreeSpecialMeal or AncillaryVariant.Wheelchair && paid)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(Outcome), variant.Code);

            if (definition.Upgrade is { } upgrade && paid && (upgrade.AllowedUpgradeKind == UpgradeKind.FixedAncillary) != (PriceOrigin == PriceOrigin.Filed))
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(PriceOrigin), variant.Code);

            if (variant.Code is AncillaryVariant.StandardSeat or AncillaryVariant.PreferredSeat && Quantity.MaxQuantity != 1)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(nameof(Quantity), variant.Code);
        }

        private void EnsureRulesOf(AncillaryServiceDefinition definition, AncillaryVariant variant)
        {
            void Refuse(bool refused, string field)
            {
                if (refused)
                    throw ExceptionFactory.ProvisionProfileRuleNotAllowed(field, variant.Code);
            }

            Refuse(PetRule is not null && definition.Pet is null, nameof(PetRule));
            Refuse(AssistedTravelRule is not null && definition.AssistedTravel is null, nameof(AssistedTravelRule));
            Refuse(AirportServiceRule is not null && definition.AirportService is null, nameof(AirportServiceRule));

            if (BaggageApplication is { } baggage && definition.Baggage is { } baggageSpecification)
            {
                Refuse(baggage.ChargeKind is { } chargeKind && chargeKind != baggageSpecification.ChargeKind, nameof(BaggageApplication));
                Refuse(
                    baggage.AllowanceConcept is { } concept && baggageSpecification.AllowanceConcept is { } specified && concept != specified,
                    nameof(BaggageApplication));
            }

            if (definition.Upgrade is { } upgrade && FareApplication is { CabinClasses.Count: > 0 } fare)
                Refuse(fare.CabinClasses.All(cabin => cabin.CabinClassId != upgrade.FromCabinId), nameof(FareApplication));

            if (PetRule is { } petRule && definition.Pet is { } pet)
            {
                Refuse(petRule.AcceptanceMode != ConfirmationRequirement.SubjectToConfirmation, nameof(PetRule));
                Refuse(petRule.MaxCombinedKgOverride > pet.MaxCombinedWeightKg, nameof(PetRule));
                Refuse(petRule.MinAnimalAgeWeeksOverride < (pet.MinAnimalAgeWeeks ?? 0), nameof(PetRule));
            }

            if (AssistedTravelRule is { } assistedRule && definition.AssistedTravel is { } assisted)
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

            if (AirportServiceRule is { } airportRule && definition.AirportService is { } airport)
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
    }
}
