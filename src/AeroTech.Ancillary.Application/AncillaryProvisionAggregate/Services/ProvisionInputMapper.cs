using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services
{
    internal static class ProvisionInputMapper
    {
        public static PassengerCriteria ToCriteria(this ProvisionPassengerCriteriaInput? passenger)
            => PassengerCriteria.Create(passenger?.PassengerTypeCodes);

        public static SalesCriteria ToCriteria(this ProvisionSalesCriteriaInput? sales)
            => SalesCriteria.Create(sales?.PointOfSaleIds, sales?.CustomerIds, sales?.CustomerTypes);

        public static TravelCriteria ToCriteria(this ProvisionTravelCriteriaInput? travel)
            => TravelCriteria.Create(
                travel?.OriginAirportIds,
                travel?.DestinationAirportIds,
                travel?.ViaAirportIds,
                travel?.TravelFrom,
                travel?.TravelTo,
                travel?.DaysOfWeek,
                travel?.TimeFrom,
                travel?.TimeTo,
                travel?.MarketingAirlineIds,
                travel?.OperatingAirlineIds,
                travel?.FlightNumbers,
                travel?.FlightIds,
                travel?.AircraftIds);

        public static IReadOnlyList<ProvisionRoutePairArgs> ToRoutePairs(this ProvisionTravelCriteriaInput? travel)
            => travel?.RoutePairs?
                .Select(pair => new ProvisionRoutePairArgs(pair.OriginAirportId, pair.DestinationAirportId, pair.Direction))
                .ToList() ?? [];

        public static FareCriteria ToCriteria(this ProvisionFareCriteriaInput? fare)
            => FareCriteria.Create(
                fare?.AirFareIds,
                fare?.AirFareTypes,
                fare?.FareFamilyIds,
                fare?.FareBasisCodes,
                fare?.CabinClassIds,
                fare?.RbdIds);

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
                    : null,
                application.Seat is { } seat
                    ? SeatApplication.Create(seat.SeatNumbers, seat.SeatCharacteristicCodes)
                    : null);

        public static CommercialOutcome ToOutcome(this ProvisionOutcomeInput outcome)
            => CommercialOutcome.Create(outcome.Disposition, outcome.DocumentRequired, outcome.BookingRequired);

        public static FeeDefinition? ToDefinition(this ProvisionFeeInput? fee)
            => fee is null ? null : FeeDefinition.Create(fee.CurrencyId, fee.ApplicationUnit);

        public static IReadOnlyList<ProvisionPriceLineArgs> ToPriceLines(this ProvisionFeeInput? fee)
            => fee?.PriceLines
                .Select(line => new ProvisionPriceLineArgs(
                    line.Category,
                    line.Code,
                    line.Name,
                    line.UnitAmount,
                    line.CountryId,
                    line.StationAirportId))
                .ToList() ?? [];

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
