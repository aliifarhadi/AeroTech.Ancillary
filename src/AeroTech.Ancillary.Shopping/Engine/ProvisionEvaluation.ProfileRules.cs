using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed partial class ProvisionEvaluation
    {
        private static readonly string[] CoverageFields = ["TravellerRef", "BoundRef", "FlightRef", "ChildRef"];

        private static readonly IReadOnlyDictionary<string, Func<AncillarySelection, bool>> ProvidedFields =
            new Dictionary<string, Func<AncillarySelection, bool>>(StringComparer.Ordinal)
            {
                ["Quantity"] = selection => selection.Quantity is not null,
                ["PackageProductRef"] = selection => selection.PackageProductRef is not null,
                ["BagRef"] = selection => selection.BagRef is not null,
                ["MeasuredWeightKg"] = selection => selection.MeasuredWeightKg is not null,
                ["Dimensions"] = selection => selection.Dimensions is not null,
                ["EquipmentKind"] = selection => selection.EquipmentKind is not null,
                ["WeightKg"] = selection => selection.WeightKg is not null,
                ["SeatNumber"] = selection => selection.SeatNumber is not null,
                ["ExitRowTermsAccepted"] = selection => selection.ExitRowTermsAccepted is not null,
                ["Purpose"] = selection => selection.Purpose is not null,
                ["TargetCabinId"] = selection => selection.TargetCabinId is not null,
                ["QuoteRef"] = selection => selection.QuoteRef is not null,
                ["MealCode"] = selection => selection.MealCode is not null,
                ["MenuItemRef"] = selection => selection.MenuItemRef is not null,
                ["AnimalType"] = selection => selection.AnimalType is not null,
                ["CombinedWeightKg"] = selection => selection.CombinedWeightKg is not null,
                ["CarrierDimensions"] = selection => selection.CarrierDimensions is not null,
                ["DocumentAcknowledgements"] = selection => selection.DocumentAcknowledgements is not null,
                ["SizeBracket"] = selection => selection.SizeBracket is not null,
                ["AssistanceSsrCode"] = selection => selection.AssistanceSsrCode is not null,
                ["EquipmentCode"] = selection => selection.EquipmentCode is not null,
                ["EvidenceDocumentRefs"] = selection => selection.EvidenceDocumentRefs is not null,
                ["OxygenUnits"] = selection => selection.OxygenUnits is not null,
                ["InfantRef"] = selection => selection.InfantRef is not null,
                ["GuardianRef"] = selection => selection.GuardianRef is not null,
                ["ChildRef"] = selection => selection.ChildRef is not null,
                ["GuardianHandoffContact"] = selection => selection.GuardianHandoffContactProvided,
                ["GuardianPickupContact"] = selection => selection.GuardianPickupContactProvided,
                ["AirportId"] = selection => selection.AirportId is not null,
                ["FacilityRef"] = selection => selection.FacilityRef is not null,
                ["TimeWithOffset"] = selection => selection.TimeWithOffset is not null,
                ["GuestCount"] = selection => selection.GuestCount is not null,
                ["OptIn"] = selection => selection.OptIn is not null,
                ["PlanCode"] = selection => selection.PlanCode is not null,
                ["DeviceCount"] = selection => selection.DeviceCount is not null
            };

        private void SelectionContract()
        {
            var contract = _definition.SelectionContract!;

            if (Selection is null)
            {
                if (contract.Kind is SelectionKind.TypedForm or SelectionKind.SeatMapSelection)
                    Outcome.AwaitForm(ShoppingReasonCodes.SelectionRequired, nameof(Selection));

                return;
            }

            foreach (var field in contract.Fields.Where(field => field.Required && !CoverageFields.Contains(field.Name) && !ProvidedFields[field.Name](Selection)))
                Outcome.AwaitForm(ShoppingReasonCodes.SelectionFieldRequired, $"Selection.{field.Name}");

            foreach (var name in ProvidedFields.Where(field => field.Value(Selection) && contract.Fields.All(allowed => allowed.Name != field.Key)).Select(field => field.Key))
                Outcome.Reject(name, ShoppingReasonCodes.SelectionFieldNotAllowed);

            if (Selection.ChildRef is { } child && child != _unit.Traveller.TravellerRef)
                Outcome.Reject("ChildRef", ShoppingReasonCodes.SelectionFieldNotAllowed);

            if (Selection.Quantity is not { } quantity)
                return;

            if (quantity < _provision.Quantity.MinQuantity)
                Outcome.Reject("Quantity", ShoppingReasonCodes.QuantityBelowMinimum);
            else if (quantity > _provision.Quantity.MaxQuantity)
                Outcome.Reject("Quantity", ShoppingReasonCodes.QuantityAboveMaximum);
        }

        private void ProfileRules()
        {
            switch (_definition.Profile)
            {
                case AncillaryProfile.Baggage:
                    Baggage(_definition.Baggage!);

                    break;
                case AncillaryProfile.Seat:
                    Seat(_definition.Seat!);

                    break;
                case AncillaryProfile.Upgrade:
                    Upgrade(_definition.Upgrade!);

                    break;
                case AncillaryProfile.Meal:
                    Meal(_definition.Meal!);

                    break;
                case AncillaryProfile.Pet:
                    Pet(_definition.Pet!);

                    break;
                case AncillaryProfile.AssistedTravel:
                    AssistedTravel(_definition.AssistedTravel!);

                    break;
                case AncillaryProfile.AirportService:
                    AirportService(_definition.AirportService!);

                    break;
                case AncillaryProfile.Priority:
                    Priority(_definition.Priority!);

                    break;
                case AncillaryProfile.Connectivity:
                    Connectivity(_definition.Connectivity!);

                    break;
            }
        }

        private static bool Fits(SelectedDimensions selected, DimensionsCm maximum)
        {
            decimal[] actual = [selected.LengthCm, selected.WidthCm, selected.HeightCm];
            decimal[] allowed = [maximum.LengthCm, maximum.WidthCm, maximum.HeightCm];

            return actual.OrderDescending().Zip(allowed.OrderDescending(), (size, limit) => size > 0 && size <= limit).All(fits => fits);
        }

        private int? CabinOf(ShoppingFlight flight) => _unit.FareOf(flight)?.CabinClassId ?? flight.CabinClassId;

        private void Baggage(BaggageSpecification specification)
        {
            if (Selection is null)
                return;

            if (Selection.MeasuredWeightKg is { } measured
                && (measured <= (specification.WeightFromExclusiveKg ?? 0m) || measured > (specification.WeightToInclusiveKg ?? decimal.MaxValue)))
                Outcome.Reject("MeasuredWeightKg", ShoppingReasonCodes.BaggageWeightOutsideBracket);

            if (Selection.Dimensions is { } dimensions
                && ((specification.MaxSize is { } maximum && !Fits(dimensions, maximum))
                    || (specification.MaxLinearSumCm is { } linear && dimensions.LengthCm + dimensions.WidthCm + dimensions.HeightCm > linear)))
                Outcome.Reject("Dimensions", ShoppingReasonCodes.BaggageDimensionsAboveMaximum);

            if (Selection.WeightKg is { } weight && specification.MaxKgPerPiece is { } perPiece && weight > perPiece)
                Outcome.Reject("WeightKg", ShoppingReasonCodes.BaggageWeightAboveMaximum);

            if (Selection.EquipmentKind is { } equipment
                && specification.EquipmentKind is { } allowed
                && !string.Equals(equipment.Trim(), allowed, StringComparison.OrdinalIgnoreCase))
                Outcome.Reject("EquipmentKind", ShoppingReasonCodes.EquipmentKindNotAllowed);
        }

        private void Seat(SeatSpecification specification)
        {
            if (specification.ApplicableCabins.Count > 0)
            {
                foreach (var flight in _unit.Flights)
                {
                    if (CabinOf(flight) is not { } cabin)
                        Outcome.Unknown(ShoppingReasonCodes.CabinNotVerified, $"Flights[{flight.FlightRef}].CabinClassId");
                    else if (specification.ApplicableCabins.All(row => row.ReferenceId != cabin))
                        Outcome.Fail(ShoppingReasonCodes.CabinNotAllowed);
                }
            }

            if (specification.RequiresExitRowEligibility)
            {
                if (_unit.Traveller.VerifiedExitRowEligible is not { } eligible)
                    Outcome.Unknown(ShoppingReasonCodes.ExitRowEligibilityNotVerified, $"Travellers[{_unit.Traveller.TravellerRef}].VerifiedExitRowEligible");
                else if (!eligible)
                    Outcome.Fail(ShoppingReasonCodes.ExitRowNotEligible);
            }

            if (Selection is null)
                return;

            if (specification.RequiresExitRowEligibility && Selection.ExitRowTermsAccepted == false)
                Outcome.Reject("ExitRowTermsAccepted", ShoppingReasonCodes.ExitRowTermsNotAccepted);

            if (Selection.SeatNumber is not null && specification.SeatCharacteristicCodes.Count > 0)
            {
                if (Selection.VerifiedSeatCharacteristicCodes is not { } characteristics)
                    Outcome.Unknown(ShoppingReasonCodes.SeatMapNotVerified, "Selection.VerifiedSeatCharacteristicCodes");
                else if (!characteristics.Any(code => specification.SeatCharacteristicCodes.Any(row => string.Equals(row.Code, code, StringComparison.OrdinalIgnoreCase))))
                    Outcome.Reject("SeatNumber", ShoppingReasonCodes.SeatCharacteristicNotAllowed);
            }

            if (Selection.Purpose is { } purpose && specification.ExtraSeatPurpose is { } authored && purpose != authored)
                Outcome.Reject("Purpose", ShoppingReasonCodes.ExtraSeatPurposeMismatch);
        }

        private void Upgrade(UpgradeSpecification specification)
        {
            foreach (var flight in _unit.Flights)
            {
                if (CabinOf(flight) is not { } cabin)
                    Outcome.Unknown(ShoppingReasonCodes.CabinNotVerified, $"Flights[{flight.FlightRef}].CabinClassId");
                else if (cabin != specification.FromCabinId)
                    Outcome.Fail(ShoppingReasonCodes.UpgradeFromCabinMismatch);

                if (specification.EligibleFareFamilies.Count == 0)
                    continue;

                if (_unit.FareOf(flight)?.FareFamilyId is not { } fareFamily)
                    Outcome.Unknown(ShoppingReasonCodes.FareFamilyNotVerified, $"TravellerFareFacts[{_unit.Traveller.TravellerRef},{flight.FlightRef}].FareFamilyId");
                else if (specification.EligibleFareFamilies.All(row => row.ReferenceId != fareFamily))
                    Outcome.Fail(ShoppingReasonCodes.UpgradeFareFamilyNotEligible);
            }

            if (Selection?.TargetCabinId is { } target && target != specification.ToCabinId)
                Outcome.Reject("TargetCabinId", ShoppingReasonCodes.UpgradeTargetCabinMismatch);
        }

        private void Meal(MealSpecification specification)
        {
            if (specification.CateringLeadTimeMinutes > 0
                && Occurrences() is { } occurrences
                && occurrences.Any(occurrence => occurrence.Instant - Context.EvaluatedAtUtc < TimeSpan.FromMinutes(specification.CateringLeadTimeMinutes)))
                Outcome.Fail(ShoppingReasonCodes.CateringCutoffPassed);

            if (specification.ExclusiveMealFamilyCode is { } family)
            {
                if (!Context.CoverageCompleteness.ExistingServicesComplete)
                    Outcome.Unknown(ShoppingReasonCodes.ExistingServicesNotVerified, nameof(Context.ExistingServiceFacts));
                else if (_unit.ActiveServices().Any(service => service.CountingFamilyCode == family))
                    Outcome.Fail(ShoppingReasonCodes.MealFamilyAlreadySelected);
            }

            if (Selection?.MealCode is { } mealCode && specification.MealCode is { } authoredMeal && !string.Equals(mealCode.Trim(), authoredMeal, StringComparison.OrdinalIgnoreCase))
                Outcome.Reject("MealCode", ShoppingReasonCodes.MealCodeMismatch);

            if (Selection?.MenuItemRef is { } menuItem && specification.MenuItemRef is { } authoredItem && !string.Equals(menuItem.Trim(), authoredItem, StringComparison.OrdinalIgnoreCase))
                Outcome.Reject("MenuItemRef", ShoppingReasonCodes.MenuItemMismatch);
        }

        private void Pet(PetSpecification specification)
        {
            var rule = _provision.PetRule;

            Outcome.Note(ShoppingReasonCodes.SupplierConfirmationRequired);

            if ((rule?.MinAnimalAgeWeeksOverride ?? specification.MinAnimalAgeWeeks) is not null)
                Outcome.Note(ShoppingReasonCodes.PetAgeNotVerified);

            if (Selection is null)
                return;

            if (Selection.AnimalType is { } animal
                && !specification.AllowedAnimalTypes.Any(row => row.AnimalType == animal
                                                               && (animal != PetAnimalType.RegisteredOther
                                                                   || string.Equals(row.OtherCode, Selection.OtherAnimalCode?.Trim(), StringComparison.OrdinalIgnoreCase))))
                Outcome.Reject("AnimalType", ShoppingReasonCodes.PetAnimalNotAllowed);

            if (Selection.CombinedWeightKg is { } weight)
            {
                if (weight <= 0 || weight > (rule?.MaxCombinedKgOverride ?? specification.MaxCombinedWeightKg))
                    Outcome.Reject("CombinedWeightKg", ShoppingReasonCodes.PetWeightAboveMaximum);

                if (specification.AllowedHoldAnimalSizeBrackets.Count > 0
                    && Selection.SizeBracket is { } bracketCode
                    && !specification.AllowedHoldAnimalSizeBrackets.Any(bracket => string.Equals(bracket.Code, bracketCode.Trim(), StringComparison.OrdinalIgnoreCase)
                                                                                   && weight > bracket.WeightFromExclusiveKg
                                                                                   && weight <= bracket.WeightToInclusiveKg))
                    Outcome.Reject("SizeBracket", ShoppingReasonCodes.PetSizeBracketMismatch);
            }

            if (Selection.CarrierDimensions is { } carrier && !Fits(carrier, specification.CarrierDimensionsMaxCm))
                Outcome.Reject("CarrierDimensions", ShoppingReasonCodes.PetCarrierAboveMaximum);

            if (Selection.DocumentAcknowledgements is { } acknowledged
                && specification.RequiredDocumentCodes.Any(required => !acknowledged.Contains(required.Code, StringComparer.OrdinalIgnoreCase)))
                Outcome.Reject("DocumentAcknowledgements", ShoppingReasonCodes.PetDocumentsNotAcknowledged);
        }

        private void AssistedTravel(AssistedTravelSpecification specification)
        {
            var rule = _provision.AssistedTravelRule;
            var leadMinutes = Math.Max(rule?.MinimumLeadTimeMinutes ?? 0, specification.Wheelchair?.LeadTimeMinutes ?? 0);

            if (leadMinutes > 0
                && Occurrences() is { } occurrences
                && occurrences.Any(occurrence => occurrence.Instant - Context.EvaluatedAtUtc < TimeSpan.FromMinutes(leadMinutes)))
                Outcome.Fail(ShoppingReasonCodes.AssistanceLeadTimeTooShort);

            if (_definition.Booking.ConfirmationRequirement == ConfirmationRequirement.SubjectToConfirmation)
                Outcome.Note(ShoppingReasonCodes.SupplierConfirmationRequired);

            if (specification.UnaccompaniedMinor is { } minor)
                UnaccompaniedMinor(minor, rule?.ConnectionPolicy ?? minor.ConnectionPolicy);

            if (specification.Bassinet is { } bassinet)
                Bassinet(bassinet);

            var allowedCodes = specification.Wheelchair?.AllowedSsrCodes ?? specification.DisabilityAssistance?.AllowedSsrCodes;

            if (Selection?.AssistanceSsrCode is { } ssrCode
                && allowedCodes is { Count: > 0 }
                && !allowedCodes.Any(row => string.Equals(row.Code, ssrCode.Trim(), StringComparison.OrdinalIgnoreCase)))
                Outcome.Reject("AssistanceSsrCode", ShoppingReasonCodes.AssistanceCodeNotAllowed);

            if (specification.MedicalEquipment is not { } medical)
                return;

            if (rule?.MedicalApprovalRequired ?? medical.RequiresMedicalApproval)
                Outcome.Note(ShoppingReasonCodes.MedicalApprovalRequired);

            if (Selection is null)
                return;

            if (Selection.EquipmentCode is { } equipment && !string.Equals(equipment.Trim(), medical.MedicalServiceCode, StringComparison.OrdinalIgnoreCase))
                Outcome.Reject("EquipmentCode", ShoppingReasonCodes.AssistanceCodeNotAllowed);

            if (Selection.EvidenceDocumentRefs is { } evidence && evidence.Count < medical.EvidenceTypeCodes.Count)
                Outcome.Reject("EvidenceDocumentRefs", ShoppingReasonCodes.MedicalEvidenceMissing);

            if (Selection.OxygenUnits is { } oxygen && (oxygen <= 0 || oxygen > (medical.OxygenUnits ?? decimal.MaxValue)))
                Outcome.Reject("OxygenUnits", ShoppingReasonCodes.OxygenUnitsAboveMaximum);
        }

        private void UnaccompaniedMinor(UnaccompaniedMinorDetails minor, MinorConnectionPolicy policy)
        {
            if (Ages() is { } ages && ages.Any(age => age < minor.MinAgeYears || age >= minor.MaxAgeYearsExclusive))
                Outcome.Fail(ShoppingReasonCodes.MinorAgeOutsideBand);

            if (_unit.Flights.Count == 1)
                return;

            var transit = _unit.Portions.SelectMany(portion => portion.FlightRefs.SkipLast(1))
                .Select(flightRef => _unit.Flights.First(flight => flight.FlightRef == flightRef).DestinationAirportId)
                .ToList();

            if (policy == MinorConnectionPolicy.DirectOnly && transit.Count > 0)
                Outcome.Fail(ShoppingReasonCodes.MinorConnectionNotAllowed);
            else if (minor.AllowedTransitAirports.Count > 0 && transit.Any(airport => minor.AllowedTransitAirports.All(row => row.ReferenceId != airport)))
                Outcome.Fail(ShoppingReasonCodes.MinorConnectionNotAllowed);
        }

        private void Bassinet(BassinetDetails bassinet)
        {
            if (Selection is not { InfantRef: { } infantRef, GuardianRef: { } guardianRef })
                return;

            var infant = Context.Travellers.FirstOrDefault(traveller => traveller.TravellerRef == infantRef);
            var guardian = Context.Travellers.FirstOrDefault(traveller => traveller.TravellerRef == guardianRef);

            if (infant is null || guardian is null || infantRef == guardianRef || (bassinet.RequiresInfantAndGuardian && infant.AssociatedAdultRef != guardianRef))
            {
                Outcome.Reject("GuardianRef", ShoppingReasonCodes.InfantGuardianRequired);

                return;
            }

            if (bassinet.MaxInfantAgeMonths is not { } maximumMonths || Occurrences() is not { } occurrences)
                return;

            if (infant.DateOfBirth is not { } dateOfBirth)
                Outcome.Unknown(ShoppingReasonCodes.AgeNotVerified, $"Travellers[{infantRef}].DateOfBirth");
            else if (occurrences.Any(occurrence => ServiceOccurrence.CompletedMonths(dateOfBirth, occurrence.LocalDate) > maximumMonths))
                Outcome.Reject("InfantRef", ShoppingReasonCodes.InfantTooOld);
        }

        private void AirportService(AirportServiceSpecification specification)
        {
            var rule = _provision.AirportServiceRule;
            var direction = rule?.Direction ?? specification.Direction;
            var departures = _unit.Flights.Select(flight => flight.OriginAirportId).ToList();
            var arrivals = _unit.Flights.Select(flight => flight.DestinationAirportId).ToList();
            var transfers = _unit.Portions.SelectMany(portion => portion.FlightRefs.SkipLast(1))
                .Select(flightRef => _unit.Flights.First(flight => flight.FlightRef == flightRef).DestinationAirportId)
                .ToList();
            var onItinerary = direction switch
            {
                AirportServiceDirection.Departure => departures.Contains(specification.AirportId),
                AirportServiceDirection.Arrival => arrivals.Contains(specification.AirportId),
                AirportServiceDirection.Transfer => transfers.Contains(specification.AirportId),
                _ => departures.Contains(specification.AirportId) || arrivals.Contains(specification.AirportId)
            };

            if (!onItinerary)
                Outcome.Fail(ShoppingReasonCodes.AirportNotOnItinerary);

            if (_definition.Booking.ConfirmationRequirement == ConfirmationRequirement.SubjectToConfirmation)
                Outcome.Note(ShoppingReasonCodes.SupplierConfirmationRequired);

            if (Selection is null)
                return;

            if (Selection.AirportId is { } airport && airport != specification.AirportId)
                Outcome.Reject("AirportId", ShoppingReasonCodes.AirportMismatch);

            if (Selection.FacilityRef is { } facility && (rule?.FacilityId ?? specification.FacilityId) is { } authoredFacility && facility != authoredFacility)
                Outcome.Reject("FacilityRef", ShoppingReasonCodes.FacilityMismatch);

            if (Selection.GuestCount is { } guests && (guests < 0 || guests > (rule?.MaxGuestsPerPrimary ?? specification.MaxGuestsPerPrimary ?? 0)))
                Outcome.Reject("GuestCount", ShoppingReasonCodes.GuestsAboveMaximum);

            var from = rule?.ServiceWindowStart ?? specification.ServiceWindowStart;
            var until = rule?.ServiceWindowEnd ?? specification.ServiceWindowEnd;

            if (Selection.TimeWithOffset is not { } time || (from is null && until is null))
                return;

            if (!ServiceOccurrence.TryResolve(time, specification.IanaTimeZone, out var appointment))
            {
                Outcome.Unknown(ShoppingReasonCodes.TimeZoneNotVerified, "Specification.IanaTimeZone");

                return;
            }

            var local = TimeOnly.FromDateTime(appointment.Local);

            if ((from is not null && local < from.Value) || (until is not null && local >= until.Value))
                Outcome.Reject("TimeWithOffset", ShoppingReasonCodes.AppointmentOutsideServiceWindow);
        }

        private void Priority(PrioritySpecification specification)
        {
            if (specification.Airports.Count > 0 && _unit.Flights.Any(flight => specification.Airports.All(row => row.ReferenceId != flight.OriginAirportId)))
                Outcome.Fail(ShoppingReasonCodes.AirportNotOnItinerary);

            if (specification.FareBenefitRef is not { } benefit)
                return;

            foreach (var flight in _unit.Flights)
            {
                var facts = Context.FareEntitlementFacts
                    .Where(fact => fact.TravellerRef == _unit.Traveller.TravellerRef
                                   && fact.FlightRefs.Contains(flight.FlightRef, StringComparer.Ordinal)
                                   && string.Equals(fact.BenefitCode, benefit, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (facts.Any(fact => fact is { IncludedOrEntitled: true, EvidenceCompleteness: FactEvidence.Verified }))
                    Outcome.Fail(ShoppingReasonCodes.AlreadyIncludedInFare);
                else if (!Context.CoverageCompleteness.FareBenefitsComplete || facts.Any(fact => fact.EvidenceCompleteness != FactEvidence.Verified))
                    Outcome.Unknown(ShoppingReasonCodes.FareBenefitsNotVerified, nameof(Context.FareEntitlementFacts));
            }
        }

        private void Connectivity(ConnectivitySpecification specification)
        {
            if (specification.EligibleAircraft.Count > 0)
            {
                foreach (var flight in _unit.Flights)
                {
                    if (flight.AircraftId is not { } aircraft)
                        Outcome.Unknown(ShoppingReasonCodes.AircraftNotVerified, $"Flights[{flight.FlightRef}].AircraftId");
                    else if (specification.EligibleAircraft.All(row => row.ReferenceId != aircraft))
                        Outcome.Fail(ShoppingReasonCodes.AircraftNotEquipped);
                }
            }

            if (Selection?.DeviceCount is { } devices && (devices < 1 || devices > (specification.MaxDevices ?? int.MaxValue)))
                Outcome.Reject("DeviceCount", ShoppingReasonCodes.DevicesAboveMaximum);
        }
    }
}
