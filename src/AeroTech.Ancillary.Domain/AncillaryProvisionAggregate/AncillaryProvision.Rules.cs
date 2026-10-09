using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        public void ChangePassengerEligibility(ProvisionPassengerEligibilityArgs? passengerEligibility, IIdGenerator idGenerator)
        {
            EnsureDraft();

            PassengerEligibility = ProvisionPassengerEligibilityRule.Plan(PassengerEligibility, Id, passengerEligibility, idGenerator)();
        }

        public void ChangeSalesRestrictions(ProvisionSalesRestrictionsArgs? salesRestrictions, IIdGenerator idGenerator)
        {
            EnsureDraft();

            SalesRestrictions = ProvisionSalesRestrictionsRule.Plan(SalesRestrictions, Id, salesRestrictions, idGenerator)();
        }

        public void ChangeGeography(ProvisionGeographyArgs? geography, IIdGenerator idGenerator)
        {
            EnsureDraft();

            Geography = ProvisionGeographyRule.Plan(Geography, Id, geography, idGenerator)();
        }

        public void ChangeFareApplication(ProvisionFareApplicationArgs? fareApplication, IIdGenerator idGenerator)
        {
            EnsureDraft();

            FareApplication = ProvisionFareApplicationRule.Plan(FareApplication, Id, fareApplication, idGenerator)();
        }

        public void ChangeTravelDate(ProvisionTravelDateArgs? travelDate, IIdGenerator idGenerator)
        {
            EnsureDraft();

            TravelDate = ProvisionTravelDateRule.Plan(TravelDate, Id, travelDate, idGenerator)();
        }

        public void ChangeDayTimeApplication(ProvisionDayTimeApplicationArgs? dayTimeApplication, IIdGenerator idGenerator)
        {
            EnsureDraft();

            DayTimeApplication = ProvisionDayTimeApplicationRule.Plan(DayTimeApplication, Id, dayTimeApplication, idGenerator)();
        }

        public void ChangeAdvancePurchase(ProvisionAdvancePurchaseArgs? advancePurchase, IIdGenerator idGenerator)
        {
            EnsureDraft();

            AdvancePurchase = ProvisionAdvancePurchaseRule.Plan(AdvancePurchase, Id, advancePurchase, idGenerator)();
        }

        public void ChangeFlightApplication(ProvisionFlightApplicationArgs? flightApplication, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var commit = ProvisionFlightApplicationRule.Plan(FlightApplication, Id, flightApplication, idGenerator);

            EnsureApplicationRules(
                ApplicationType,
                BaggageApplication is not null,
                SeatApplication?.SeatNumbers.Count ?? 0,
                SeatApplication?.SeatCharacteristics.Count ?? 0,
                flightApplication?.AircraftIds.Count ?? 0);

            FlightApplication = commit();
        }

        public void ChangeBaggageApplication(ProvisionBaggageApplicationArgs? baggageApplication, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var commit = ProvisionBaggageApplicationRule.Plan(BaggageApplication, Id, baggageApplication, idGenerator);

            EnsureApplicationRules(
                ApplicationType,
                baggageApplication is not null,
                SeatApplication?.SeatNumbers.Count ?? 0,
                SeatApplication?.SeatCharacteristics.Count ?? 0,
                FlightApplication?.Aircraft.Count ?? 0);

            BaggageApplication = commit();
        }

        public void ChangeSeatApplication(ProvisionSeatApplicationArgs? seatApplication, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var commit = ProvisionSeatApplicationRule.Plan(SeatApplication, Id, seatApplication, idGenerator);

            EnsureApplicationRules(
                ApplicationType,
                BaggageApplication is not null,
                seatApplication?.SeatNumbers.Count ?? 0,
                seatApplication?.SeatCharacteristicCodes.Count ?? 0,
                FlightApplication?.Aircraft.Count ?? 0);

            SeatApplication = commit();
        }

        public void ChangePetRule(AncillaryServiceDefinition definition, ProvisionPetRuleArgs? petRule, IIdGenerator idGenerator)
        {
            EnsureDraft();
            EnsureProfileRule(definition, petRule is not null && definition.Pet is null, nameof(PetRule));

            PetRule = ProvisionPetRule.Plan(PetRule, Id, petRule, idGenerator)();
        }

        public void ChangeAssistedTravelRule(AncillaryServiceDefinition definition, ProvisionAssistedTravelRuleArgs? assistedTravelRule, IIdGenerator idGenerator)
        {
            EnsureDraft();
            EnsureProfileRule(definition, assistedTravelRule is not null && definition.AssistedTravel is null, nameof(AssistedTravelRule));

            AssistedTravelRule = ProvisionAssistedTravelRule.Plan(AssistedTravelRule, Id, assistedTravelRule, idGenerator)();
        }

        public void ChangeAirportServiceRule(AncillaryServiceDefinition definition, ProvisionAirportServiceRuleArgs? airportServiceRule, IIdGenerator idGenerator)
        {
            EnsureDraft();
            EnsureProfileRule(definition, airportServiceRule is not null && definition.AirportService is null, nameof(AirportServiceRule));

            AirportServiceRule = ProvisionAirportServiceRule.Plan(AirportServiceRule, Id, airportServiceRule, idGenerator)();
        }

        private void EnsureProfileRule(AncillaryServiceDefinition definition, bool foreign, string rule)
        {
            Require(definition.Id == ServiceDefinitionId, nameof(ServiceDefinitionId));

            if (foreign)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(rule, definition.VariantCode);
        }

        public ProvisionPermittedTravelPeriod AddPermittedTravelPeriod(ProvisionDatePeriodArgs period, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var rule = TravelDate ?? ProvisionTravelDateRule.Open(Id, idGenerator);
            var row = rule.AddPermittedPeriod(period, idGenerator);

            TravelDate = rule;

            return row;
        }

        public ProvisionPermittedTravelPeriod ChangePermittedTravelPeriod(long rowId, ProvisionDatePeriodArgs period)
        {
            EnsureDraft();

            return (TravelDate ?? throw ExceptionFactory.ProvisionConditionNotFound()).ChangePermittedPeriod(rowId, period);
        }

        public void RemovePermittedTravelPeriod(long rowId)
        {
            EnsureDraft();

            var rule = TravelDate ?? throw ExceptionFactory.ProvisionConditionNotFound();

            rule.RemovePermittedPeriod(rowId);

            if (rule.IsEmpty)
                TravelDate = null;
        }

        public ProvisionBlackoutPeriod AddBlackoutPeriod(ProvisionDatePeriodArgs period, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var rule = TravelDate ?? ProvisionTravelDateRule.Open(Id, idGenerator);
            var row = rule.AddBlackoutPeriod(period, idGenerator);

            TravelDate = rule;

            return row;
        }

        public ProvisionBlackoutPeriod ChangeBlackoutPeriod(long rowId, ProvisionDatePeriodArgs period)
        {
            EnsureDraft();

            return (TravelDate ?? throw ExceptionFactory.ProvisionConditionNotFound()).ChangeBlackoutPeriod(rowId, period);
        }

        public void RemoveBlackoutPeriod(long rowId)
        {
            EnsureDraft();

            var rule = TravelDate ?? throw ExceptionFactory.ProvisionConditionNotFound();

            rule.RemoveBlackoutPeriod(rowId);

            if (rule.IsEmpty)
                TravelDate = null;
        }

        public ProvisionDayTimeWindow AddDayTimeWindow(ProvisionDayTimeWindowArgs window, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var rule = DayTimeApplication ?? ProvisionDayTimeApplicationRule.Open(Id, idGenerator);
            var row = rule.AddWindow(window, idGenerator);

            DayTimeApplication = rule;

            return row;
        }

        public ProvisionDayTimeWindow ChangeDayTimeWindow(long rowId, ProvisionDayTimeWindowArgs window)
        {
            EnsureDraft();

            return (DayTimeApplication ?? throw ExceptionFactory.ProvisionConditionNotFound()).ChangeWindow(rowId, window);
        }

        public void RemoveDayTimeWindow(long rowId)
        {
            EnsureDraft();

            var rule = DayTimeApplication ?? throw ExceptionFactory.ProvisionConditionNotFound();

            rule.RemoveWindow(rowId);

            if (rule.IsEmpty)
                DayTimeApplication = null;
        }

        internal static List<TRow> MergeRows<TRow>(
            List<TRow> stored,
            List<TRow> candidates,
            Func<TRow, TRow, bool> same,
            Func<TRow, TRow, bool> conflicts,
            string field)
            where TRow : class
        {
            for (var index = 1; index < candidates.Count; index++)
            {
                var candidate = candidates[index];

                if (candidates.Take(index).Any(previous => conflicts(previous, candidate)))
                    throw ExceptionFactory.ProvisionIsInvalid(field);
            }

            return candidates
                .Select(candidate => stored.FirstOrDefault(row => same(row, candidate)) ?? candidate)
                .ToList();
        }

        internal static void ReplaceRows<TRow>(List<TRow> stored, List<TRow> rows)
        {
            stored.Clear();
            stored.AddRange(rows);
        }
    }
}
