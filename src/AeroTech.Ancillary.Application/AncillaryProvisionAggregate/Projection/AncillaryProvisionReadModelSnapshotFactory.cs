using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;

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
                provision.SalesEffectiveFrom,
                provision.SalesDiscontinueAt,
                provision.CoverageScope,
                ToCriteria(provision),
                provision.Quantity.Unit,
                provision.Quantity.MinQuantity,
                provision.Quantity.MaxQuantity,
                ToApplication(provision),
                provision.Outcome.Disposition,
                provision.Outcome.DocumentRequired,
                provision.Outcome.BookingRequired,
                provision.Fee?.CurrencyId,
                provision.Fee?.ApplicationUnit,
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
                provision.RoutePairs
                    .OrderBy(pair => pair.Id)
                    .Select(pair => new ProvisionRoutePairReadModelSnapshot(
                        pair.Id,
                        pair.OriginAirportId,
                        pair.DestinationAirportId,
                        pair.Direction))
                    .ToList(),
                provision.PriceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new ProvisionPriceLineReadModelSnapshot(
                        line.Id,
                        line.Category,
                        line.Code,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        line.UnitAmount))
                    .ToList());

        private static ProvisionCriteriaReadModelSnapshot ToCriteria(AncillaryProvision provision)
            => new(
                provision.Passenger.PassengerTypeCodes.ToList(),
                provision.Sales.PointOfSaleIds.ToList(),
                provision.Sales.CustomerIds.ToList(),
                provision.Sales.CustomerTypes.ToList(),
                provision.Travel.OriginAirportIds.ToList(),
                provision.Travel.DestinationAirportIds.ToList(),
                provision.Travel.ViaAirportIds.ToList(),
                provision.Travel.TravelFrom,
                provision.Travel.TravelTo,
                provision.Travel.DaysOfWeek.ToList(),
                provision.Travel.TimeFrom,
                provision.Travel.TimeTo,
                provision.Travel.MarketingAirlineIds.ToList(),
                provision.Travel.OperatingAirlineIds.ToList(),
                provision.Travel.FlightNumbers.ToList(),
                provision.Travel.FlightIds.ToList(),
                provision.Travel.AircraftIds.ToList(),
                provision.Fare.AirFareIds.ToList(),
                provision.Fare.AirFareTypes.ToList(),
                provision.Fare.FareFamilyIds.ToList(),
                provision.Fare.FareBasisCodes.ToList(),
                provision.Fare.CabinClassIds.ToList(),
                provision.Fare.RbdIds.ToList(),
                provision.AdvancePurchase?.Period,
                provision.AdvancePurchase?.Unit);

        private static ProvisionApplicationReadModelSnapshot ToApplication(AncillaryProvision provision)
            => new(
                provision.Application.Type,
                provision.Application.Baggage?.FreePieces,
                provision.Application.Baggage?.FirstExcessPiece,
                provision.Application.Baggage?.LastExcessPiece,
                provision.Application.Baggage?.Weight,
                provision.Application.Baggage?.WeightUnit,
                provision.Application.Baggage?.TravelApplication,
                provision.Application.Baggage?.PurchaseApplication,
                provision.Application.Baggage?.RuleDeference,
                provision.Application.Seat?.SeatNumbers.ToList() ?? [],
                provision.Application.Seat?.SeatCharacteristicCodes.ToList() ?? []);
    }
}
