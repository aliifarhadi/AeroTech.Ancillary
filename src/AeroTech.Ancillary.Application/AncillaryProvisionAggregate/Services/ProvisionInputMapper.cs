using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services
{
    internal static class ProvisionInputMapper
    {
        public static QuantityRule ToRule(this ProvisionQuantityInput quantity)
            => QuantityRule.Create(quantity.Unit, quantity.MinQuantity, quantity.MaxQuantity);

        public static CommercialOutcome ToOutcome(this ProvisionOutcomeInput outcome)
            => CommercialOutcome.Create(outcome.Disposition, outcome.DocumentRequired, outcome.BookingRequired);

        public static SettlementDefinition ToDefinition(this ProvisionSettlementInput settlement)
            => SettlementDefinition.Create(settlement.ReissueRefund, settlement.FormOfRefund, settlement.Commissionable, settlement.InterlineSettlement);

        public static AvailabilityDefinition ToDefinition(this ProvisionAvailabilityInput availability)
            => AvailabilityDefinition.Create(availability.MustCheckAvailability);

        public static FulfillmentDefinition ToDefinition(this ProvisionFulfillmentInput fulfillment)
            => FulfillmentDefinition.Create(fulfillment.FulfillmentProviderKey);

        public static ProvisionDayTimeWindowArgs ToArgs(this ProvisionDayTimeWindowInput window)
            => new(window.DaysOfWeekMask, window.StartLocalTime, window.EndLocalTime, window.Effect);

        public static ProvisionRulesArgs ToRules(
            ProvisionPassengerEligibilityInput? passengerEligibility,
            ProvisionSalesRestrictionsInput? salesRestrictions,
            ProvisionGeographyInput? geography,
            ProvisionFlightApplicationInput? flightApplication,
            ProvisionFareApplicationInput? fareApplication,
            ProvisionTravelDateInput? travelDate,
            ProvisionDayTimeApplicationInput? dayTimeApplication,
            ProvisionAdvancePurchaseInput? advancePurchase,
            ProvisionBaggageApplicationInput? baggageApplication,
            ProvisionSeatApplicationInput? seatApplication,
            ProvisionPetRuleInput? petRule,
            ProvisionAssistedTravelRuleInput? assistedTravelRule,
            ProvisionAirportServiceRuleInput? airportServiceRule)
            => new(
                passengerEligibility.ToArgs(),
                salesRestrictions.ToArgs(),
                geography.ToArgs(),
                flightApplication.ToArgs(),
                fareApplication.ToArgs(),
                travelDate.ToArgs(),
                dayTimeApplication.ToArgs(),
                advancePurchase.ToArgs(),
                baggageApplication.ToArgs(),
                seatApplication.ToArgs(),
                petRule.ToArgs(),
                assistedTravelRule.ToArgs(),
                airportServiceRule.ToArgs());

        public static ProvisionPassengerEligibilityArgs? ToArgs(this ProvisionPassengerEligibilityInput? input)
            => input is null
                ? null
                : new ProvisionPassengerEligibilityArgs(
                    input.AllowedPassengerTypes ?? [],
                    (input.AllowedAgeBands ?? []).Select(band => new ProvisionAgeBandArgs(band.AgeFromInclusive, band.AgeToExclusive)).ToList());

        public static ProvisionSalesRestrictionsArgs? ToArgs(this ProvisionSalesRestrictionsInput? input)
            => input is null
                ? null
                : new ProvisionSalesRestrictionsArgs(
                    input.SalesEffectiveFrom,
                    input.SalesDiscontinueAt,
                    input.AllowedPointOfSaleIds ?? [],
                    input.AllowedCustomerIds ?? [],
                    input.AllowedCustomerTypes ?? []);

        public static ProvisionGeographyArgs? ToArgs(this ProvisionGeographyInput? input)
            => input is null
                ? null
                : new ProvisionGeographyArgs(
                    input.AllowedOriginAirportIds ?? [],
                    input.AllowedDestinationAirportIds ?? [],
                    input.AllowedViaAirportIds ?? [],
                    (input.AllowedRoutePairs ?? []).Select(pair => new ProvisionRoutePairArgs(pair.OriginAirportId, pair.DestinationAirportId, pair.Direction)).ToList(),
                    (input.ServiceLocations ?? []).Select(location => new ProvisionServiceLocationArgs(location.LocationType, location.LocationId)).ToList(),
                    input.CoverageCountryIds ?? []);

        public static ProvisionFlightApplicationArgs? ToArgs(this ProvisionFlightApplicationInput? input)
            => input is null
                ? null
                : new ProvisionFlightApplicationArgs(
                    input.AllowedMarketingAirlineIds ?? [],
                    input.AllowedOperatingAirlineIds ?? [],
                    input.AllowedFlightNumbers ?? [],
                    input.AllowedFlightIds ?? [],
                    input.AllowedAircraftIds ?? []);

        public static ProvisionFareApplicationArgs? ToArgs(this ProvisionFareApplicationInput? input)
            => input is null
                ? null
                : new ProvisionFareApplicationArgs(
                    input.AllowedAirFareIds ?? [],
                    input.AllowedAirFareTypes ?? [],
                    input.AllowedFareFamilyIds ?? [],
                    input.AllowedFareBasisCodes ?? [],
                    input.AllowedCabinClassIds ?? [],
                    input.AllowedRbdIds ?? []);

        public static ProvisionTravelDateArgs? ToArgs(this ProvisionTravelDateInput? input)
            => input is null
                ? null
                : new ProvisionTravelDateArgs(
                    (input.PermittedPeriods ?? []).Select(period => new ProvisionDatePeriodArgs(period.StartDate, period.EndDate)).ToList(),
                    (input.BlackoutPeriods ?? []).Select(period => new ProvisionDatePeriodArgs(period.StartDate, period.EndDate)).ToList());

        public static ProvisionDayTimeApplicationArgs? ToArgs(this ProvisionDayTimeApplicationInput? input)
            => input is null
                ? null
                : new ProvisionDayTimeApplicationArgs((input.Windows ?? []).Select(window => window.ToArgs()).ToList());

        public static ProvisionAdvancePurchaseArgs? ToArgs(this ProvisionAdvancePurchaseInput? input)
            => input is null
                ? null
                : new ProvisionAdvancePurchaseArgs(input.MinimumPeriod, input.Unit, input.SameTimeAsTicketed, input.MaximumPeriod);

        public static ProvisionBaggageApplicationArgs? ToArgs(this ProvisionBaggageApplicationInput? input)
            => input is null
                ? null
                : new ProvisionBaggageApplicationArgs(
                    input.FreePieces,
                    input.FirstExcessPiece,
                    input.LastExcessPiece,
                    input.Weight,
                    input.WeightUnit,
                    input.TravelApplication,
                    input.PurchaseApplication,
                    input.RuleDeference,
                    input.ChargeKind,
                    input.AllowanceConcept);

        public static ProvisionSeatApplicationArgs? ToArgs(this ProvisionSeatApplicationInput? input)
            => input is null
                ? null
                : new ProvisionSeatApplicationArgs(input.SeatNumbers ?? [], input.SeatCharacteristicCodes ?? []);

        public static ProvisionPetRuleArgs? ToArgs(this ProvisionPetRuleInput? input)
            => input is null
                ? null
                : new ProvisionPetRuleArgs(input.CountryExceptionCode, input.MinAnimalAgeWeeksOverride, input.MaxCombinedKgOverride, input.AcceptanceMode);

        public static ProvisionAssistedTravelRuleArgs? ToArgs(this ProvisionAssistedTravelRuleInput? input)
            => input is null
                ? null
                : new ProvisionAssistedTravelRuleArgs(input.MinimumLeadTimeMinutes, input.ConnectionPolicy, input.MedicalApprovalRequired);

        public static ProvisionAirportServiceRuleArgs? ToArgs(this ProvisionAirportServiceRuleInput? input)
            => input is null
                ? null
                : new ProvisionAirportServiceRuleArgs(
                    input.TerminalRef,
                    input.Direction,
                    input.ServiceWindowStart,
                    input.ServiceWindowEnd,
                    input.FacilityId,
                    input.MaxGuestsPerPrimary);
    }
}
