using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Core.Enums;

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
                provision.AdvancePurchase?.Period,
                provision.AdvancePurchase?.Unit,
                provision.Quantity.Unit,
                provision.Quantity.MinQuantity,
                provision.Quantity.MaxQuantity,
                provision.Application.Type,
                provision.Application.Baggage?.FreePieces,
                provision.Application.Baggage?.FirstExcessPiece,
                provision.Application.Baggage?.LastExcessPiece,
                provision.Application.Baggage?.Weight,
                provision.Application.Baggage?.WeightUnit,
                provision.Application.Baggage?.TravelApplication,
                provision.Application.Baggage?.PurchaseApplication,
                provision.Application.Baggage?.RuleDeference,
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
                ToConditions(provision));

        private static ProvisionConditionsReadModelSnapshot ToConditions(AncillaryProvision provision)
            => new(
                provision.PassengerTypes
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<PassengerTypeCode>(row.Id, row.PassengerTypeCode))
                    .ToList(),
                provision.PointsOfSale
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.PointOfSaleId))
                    .ToList(),
                provision.Customers
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.CustomerId))
                    .ToList(),
                provision.CustomerTypes
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<CustomerType>(row.Id, row.CustomerType))
                    .ToList(),
                provision.OriginAirports
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AirportId))
                    .ToList(),
                provision.DestinationAirports
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AirportId))
                    .ToList(),
                provision.ViaAirports
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AirportId))
                    .ToList(),
                provision.RoutePairs
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionRoutePairReadModelSnapshot(row.Id, row.OriginAirportId, row.DestinationAirportId, row.Direction))
                    .ToList(),
                provision.MarketingAirlines
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AirlineId))
                    .ToList(),
                provision.OperatingAirlines
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AirlineId))
                    .ToList(),
                provision.FlightNumbers
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<string>(row.Id, row.FlightNumber))
                    .ToList(),
                provision.Flights
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.FlightId))
                    .ToList(),
                provision.Aircraft
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.AircraftId))
                    .ToList(),
                provision.AirFares
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.AirFareId))
                    .ToList(),
                provision.AirFareTypes
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<AirFareType>(row.Id, row.AirFareType))
                    .ToList(),
                provision.FareFamilies
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.FareFamilyId))
                    .ToList(),
                provision.FareBases
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<string>(row.Id, row.FareBasisCode))
                    .ToList(),
                provision.CabinClasses
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<int>(row.Id, row.CabinClassId))
                    .ToList(),
                provision.Rbds
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<long>(row.Id, row.RbdId))
                    .ToList(),
                provision.TravelDates
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<DateOnly>(row.Id, row.TravelDate))
                    .ToList(),
                provision.SeasonalPeriods
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionDatePeriodReadModelSnapshot(row.Id, row.StartDate, row.EndDate))
                    .ToList(),
                provision.BlackoutPeriods
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionDatePeriodReadModelSnapshot(row.Id, row.StartDate, row.EndDate))
                    .ToList(),
                provision.DayTimeRestrictions
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionDayTimeRestrictionReadModelSnapshot(row.Id, row.DayOfWeek, row.StartTime, row.EndTime, row.Effect))
                    .ToList(),
                provision.SeatNumbers
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<string>(row.Id, row.SeatNumber))
                    .ToList(),
                provision.SeatCharacteristics
                    .OrderBy(row => row.Id)
                    .Select(row => new ProvisionConditionRowReadModelSnapshot<string>(row.Id, row.CharacteristicCode))
                    .ToList());
    }
}
