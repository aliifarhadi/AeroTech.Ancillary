using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection
{
    internal static class AncillaryProvisionReadModelSnapshotFactory
    {
        public static AncillaryProvisionReadModelSnapshot ToReadModelSnapshot(this AncillaryProvision provision)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                provision.Status,
                provision.CoverageScope,
                provision.Quantity.Unit,
                provision.Quantity.MinQuantity,
                provision.Quantity.MaxQuantity,
                provision.ApplicationType,
                provision.Outcome.Disposition,
                provision.Outcome.DocumentRequired,
                provision.Outcome.BookingRequired,
                provision.Settlement.ReissueRefund,
                provision.Settlement.FormOfRefund,
                provision.Settlement.Commissionable,
                provision.Settlement.InterlineSettlement,
                provision.Availability.MustCheckAvailability,
                provision.Fulfillment.FulfillmentProviderKey,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                Snapshot(provision.PassengerEligibility),
                Snapshot(provision.SalesRestrictions),
                Snapshot(provision.Geography),
                Snapshot(provision.FlightApplication),
                Snapshot(provision.FareApplication),
                Snapshot(provision.TravelDate),
                Snapshot(provision.DayTimeApplication),
                Snapshot(provision.AdvancePurchase),
                Snapshot(provision.BaggageApplication),
                Snapshot(provision.SeatApplication));

        private static ProvisionPassengerEligibilityReadModelSnapshot? Snapshot(ProvisionPassengerEligibilityRule? rule)
            => rule is null
                ? null
                : new ProvisionPassengerEligibilityReadModelSnapshot(
                    rule.Id,
                    Rows(rule.PassengerTypes, row => row.PassengerTypeCode),
                    rule.AgeBands.Select(row => new ProvisionAgeBandReadModelSnapshot(row.Id, row.AgeFromInclusive, row.AgeToExclusive)).ToList());

        private static ProvisionSalesRestrictionsReadModelSnapshot? Snapshot(ProvisionSalesRestrictionsRule? rule)
            => rule is null
                ? null
                : new ProvisionSalesRestrictionsReadModelSnapshot(
                    rule.Id,
                    rule.SalesEffectiveFrom,
                    rule.SalesDiscontinueAt,
                    Rows(rule.PointsOfSale, row => row.PointOfSaleId),
                    Rows(rule.Customers, row => row.CustomerId),
                    Rows(rule.CustomerTypes, row => row.CustomerType));

        private static ProvisionGeographyReadModelSnapshot? Snapshot(ProvisionGeographyRule? rule)
            => rule is null
                ? null
                : new ProvisionGeographyReadModelSnapshot(
                    rule.Id,
                    Rows(rule.OriginAirports, row => row.AirportId),
                    Rows(rule.DestinationAirports, row => row.AirportId),
                    Rows(rule.ViaAirports, row => row.AirportId),
                    Rows(rule.CoverageCountries, row => row.CountryId),
                    rule.RoutePairs.Select(row => new ProvisionRoutePairReadModelSnapshot(row.Id, row.OriginAirportId, row.DestinationAirportId, row.Direction)).ToList(),
                    rule.ServiceLocations.Select(row => new ProvisionServiceLocationReadModelSnapshot(row.Id, row.LocationType, row.LocationId)).ToList());

        private static ProvisionFlightApplicationReadModelSnapshot? Snapshot(ProvisionFlightApplicationRule? rule)
            => rule is null
                ? null
                : new ProvisionFlightApplicationReadModelSnapshot(
                    rule.Id,
                    Rows(rule.MarketingAirlines, row => row.AirlineId),
                    Rows(rule.OperatingAirlines, row => row.AirlineId),
                    Rows(rule.FlightNumbers, row => row.FlightNumber),
                    Rows(rule.Flights, row => row.FlightId),
                    Rows(rule.Aircraft, row => row.AircraftId));

        private static ProvisionFareApplicationReadModelSnapshot? Snapshot(ProvisionFareApplicationRule? rule)
            => rule is null
                ? null
                : new ProvisionFareApplicationReadModelSnapshot(
                    rule.Id,
                    Rows(rule.AirFares, row => row.AirFareId),
                    Rows(rule.AirFareTypes, row => row.AirFareType),
                    Rows(rule.FareFamilies, row => row.FareFamilyId),
                    Rows(rule.FareBases, row => row.FareBasisCode),
                    Rows(rule.CabinClasses, row => row.CabinClassId),
                    Rows(rule.Rbds, row => row.RbdId));

        private static ProvisionTravelDateReadModelSnapshot? Snapshot(ProvisionTravelDateRule? rule)
            => rule is null
                ? null
                : new ProvisionTravelDateReadModelSnapshot(
                    rule.Id,
                    rule.PermittedPeriods.Select(row => new ProvisionDatePeriodReadModelSnapshot(row.Id, row.StartDate, row.EndDate)).ToList(),
                    rule.BlackoutPeriods.Select(row => new ProvisionDatePeriodReadModelSnapshot(row.Id, row.StartDate, row.EndDate)).ToList());

        private static ProvisionDayTimeApplicationReadModelSnapshot? Snapshot(ProvisionDayTimeApplicationRule? rule)
            => rule is null
                ? null
                : new ProvisionDayTimeApplicationReadModelSnapshot(
                    rule.Id,
                    rule.Windows.Select(row => new ProvisionDayTimeWindowReadModelSnapshot(row.Id, row.DaysOfWeekMask, row.StartLocalTime, row.EndLocalTime, row.Effect)).ToList());

        private static ProvisionAdvancePurchaseReadModelSnapshot? Snapshot(ProvisionAdvancePurchaseRule? rule)
            => rule is null
                ? null
                : new ProvisionAdvancePurchaseReadModelSnapshot(
                    rule.Id,
                    rule.MinimumPeriod,
                    rule.Unit,
                    rule.SameTimeAsTicketed);

        private static ProvisionBaggageApplicationReadModelSnapshot? Snapshot(ProvisionBaggageApplicationRule? rule)
            => rule is null
                ? null
                : new ProvisionBaggageApplicationReadModelSnapshot(
                    rule.Id,
                    rule.FreePieces,
                    rule.FirstExcessPiece,
                    rule.LastExcessPiece,
                    rule.Weight,
                    rule.WeightUnit,
                    rule.TravelApplication,
                    rule.PurchaseApplication,
                    rule.RuleDeference);

        private static ProvisionSeatApplicationReadModelSnapshot? Snapshot(ProvisionSeatApplicationRule? rule)
            => rule is null
                ? null
                : new ProvisionSeatApplicationReadModelSnapshot(
                    rule.Id,
                    Rows(rule.SeatNumbers, row => row.SeatNumber),
                    Rows(rule.SeatCharacteristics, row => row.CharacteristicCode));

        private static IReadOnlyList<ProvisionRuleRowReadModelSnapshot<TValue>> Rows<TRow, TValue>(IEnumerable<TRow> rows, Func<TRow, TValue> value)
            where TRow : Entity<long>
            => rows.Select(row => new ProvisionRuleRowReadModelSnapshot<TValue>(row.Id, value(row))).ToList();
    }
}
