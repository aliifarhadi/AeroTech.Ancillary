using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services
{
    internal static class ProvisionInputMapper
    {
        public static ProvisionConditionsArgs ToConditions(
            ProvisionPassengerCriteriaInput? passenger,
            ProvisionSalesCriteriaInput? sales,
            ProvisionTravelCriteriaInput? travel,
            ProvisionFareCriteriaInput? fare,
            ProvisionSeatApplicationInput? seat)
            => new(
                passenger?.PassengerTypeCodes ?? [],
                sales?.PointOfSaleIds ?? [],
                sales?.CustomerIds ?? [],
                sales?.CustomerTypes ?? [],
                travel?.OriginAirportIds ?? [],
                travel?.DestinationAirportIds ?? [],
                travel?.ViaAirportIds ?? [],
                travel?.RoutePairs?
                    .Select(pair => new ProvisionRoutePairArgs(pair.OriginAirportId, pair.DestinationAirportId, pair.Direction))
                    .ToList() ?? [],
                travel?.MarketingAirlineIds ?? [],
                travel?.OperatingAirlineIds ?? [],
                travel?.FlightNumbers ?? [],
                travel?.FlightIds ?? [],
                travel?.AircraftIds ?? [],
                fare?.AirFareIds ?? [],
                fare?.AirFareTypes ?? [],
                fare?.FareFamilyIds ?? [],
                fare?.FareBasisCodes ?? [],
                fare?.CabinClassIds ?? [],
                fare?.RbdIds ?? [],
                travel?.TravelDates ?? [],
                travel?.SeasonalPeriods?.Select(period => new ProvisionDatePeriodArgs(period.StartDate, period.EndDate)).ToList() ?? [],
                travel?.BlackoutPeriods?.Select(period => new ProvisionDatePeriodArgs(period.StartDate, period.EndDate)).ToList() ?? [],
                travel?.DayTimeRestrictions?
                    .Select(restriction => new ProvisionDayTimeRestrictionArgs(
                        restriction.DayOfWeek,
                        restriction.StartTime,
                        restriction.EndTime,
                        restriction.Effect))
                    .ToList() ?? [],
                seat?.SeatNumbers ?? [],
                seat?.SeatCharacteristicCodes ?? []);

        public static AdvancePurchaseCriteria? ToCriteria(this ProvisionAdvancePurchaseInput? advancePurchase)
            => advancePurchase is null ? null : AdvancePurchaseCriteria.Create(advancePurchase.Period, advancePurchase.Unit);

        public static QuantityRule ToRule(this ProvisionQuantityInput quantity)
            => QuantityRule.Create(quantity.Unit, quantity.MinQuantity, quantity.MaxQuantity);

        public static ProvisionApplication ToApplication(this ProvisionApplicationInput application)
            => ProvisionApplication.Create(
                application.Type,
                application.Baggage is { } baggage
                    ? BaggageApplication.Create(
                        baggage.FreePieces,
                        baggage.FirstExcessPiece,
                        baggage.LastExcessPiece,
                        baggage.Weight,
                        baggage.WeightUnit,
                        baggage.TravelApplication,
                        baggage.PurchaseApplication,
                        baggage.RuleDeference)
                    : null);

        public static CommercialOutcome ToOutcome(this ProvisionOutcomeInput outcome)
            => CommercialOutcome.Create(outcome.Disposition, outcome.DocumentRequired, outcome.BookingRequired);

        public static SettlementDefinition ToDefinition(this ProvisionSettlementInput settlement)
            => SettlementDefinition.Create(
                settlement.ReissueRefund,
                settlement.FormOfRefund,
                settlement.Commissionable,
                settlement.InterlineSettlement);

        public static AvailabilityDefinition ToDefinition(this ProvisionAvailabilityInput availability)
            => AvailabilityDefinition.Create(availability.MustCheckAvailability);

        public static FulfillmentDefinition ToDefinition(this ProvisionFulfillmentInput fulfillment)
            => FulfillmentDefinition.Create(fulfillment.FulfillmentProviderKey);
    }
}
