using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionConditionsArgs(
        IReadOnlyList<PassengerTypeCode> PassengerTypeCodes,
        IReadOnlyList<long> PointOfSaleIds,
        IReadOnlyList<long> CustomerIds,
        IReadOnlyList<CustomerType> CustomerTypes,
        IReadOnlyList<int> OriginAirportIds,
        IReadOnlyList<int> DestinationAirportIds,
        IReadOnlyList<int> ViaAirportIds,
        IReadOnlyList<ProvisionRoutePairArgs> RoutePairs,
        IReadOnlyList<int> MarketingAirlineIds,
        IReadOnlyList<int> OperatingAirlineIds,
        IReadOnlyList<string> FlightNumbers,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<int> AircraftIds,
        IReadOnlyList<long> AirFareIds,
        IReadOnlyList<AirFareType> AirFareTypes,
        IReadOnlyList<long> FareFamilyIds,
        IReadOnlyList<string> FareBasisCodes,
        IReadOnlyList<int> CabinClassIds,
        IReadOnlyList<long> RbdIds,
        IReadOnlyList<DateOnly> TravelDates,
        IReadOnlyList<ProvisionDatePeriodArgs> SeasonalPeriods,
        IReadOnlyList<ProvisionDatePeriodArgs> BlackoutPeriods,
        IReadOnlyList<ProvisionDayTimeRestrictionArgs> DayTimeRestrictions,
        IReadOnlyList<string> SeatNumbers,
        IReadOnlyList<string> SeatCharacteristicCodes)
    {
        public static ProvisionConditionsArgs Unrestricted { get; } = new(
            [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []);
    }
}
