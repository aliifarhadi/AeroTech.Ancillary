using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class V121Fixtures
{
    public const byte Monday = 1;
    public const byte Tuesday = 2;
    public const byte Wednesday = 4;
    public const byte Thursday = 8;
    public const byte Friday = 16;
    public const byte Saturday = 32;
    public const byte Sunday = 64;
    public const byte Weekdays = 31;
    public const byte EveryDay = 127;

    public static DateOnly Day(int year, int month, int day) => new(year, month, day);

    public static ProvisionRulesArgs Groups(
        ProvisionPassengerEligibilityArgs? passengerEligibility = null,
        ProvisionSalesRestrictionsArgs? salesRestrictions = null,
        ProvisionGeographyArgs? geography = null,
        ProvisionFlightApplicationArgs? flightApplication = null,
        ProvisionFareApplicationArgs? fareApplication = null,
        ProvisionTravelDateArgs? travelDate = null,
        ProvisionDayTimeApplicationArgs? dayTimeApplication = null,
        ProvisionAdvancePurchaseArgs? advancePurchase = null,
        ProvisionBaggageApplicationArgs? baggageApplication = null,
        ProvisionSeatApplicationArgs? seatApplication = null)
        => new(
            passengerEligibility,
            salesRestrictions,
            geography,
            flightApplication,
            fareApplication,
            travelDate,
            dayTimeApplication,
            advancePurchase,
            baggageApplication,
            seatApplication);

    public static ProvisionPassengerEligibilityArgs Passengers(PassengerTypeCode[]? types = null, ProvisionAgeBandArgs[]? bands = null)
        => new(types ?? [], bands ?? []);

    public static ProvisionSalesRestrictionsArgs Sales(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        long[]? pointsOfSale = null,
        long[]? customers = null,
        CustomerType[]? customerTypes = null)
        => new(from, to, pointsOfSale ?? [], customers ?? [], customerTypes ?? []);

    public static ProvisionGeographyArgs Geography(
        int[]? origins = null,
        int[]? destinations = null,
        int[]? vias = null,
        ProvisionRoutePairArgs[]? pairs = null,
        ProvisionServiceLocationArgs[]? locations = null,
        int[]? countries = null)
        => new(origins ?? [], destinations ?? [], vias ?? [], pairs ?? [], locations ?? [], countries ?? []);

    public static ProvisionFlightApplicationArgs Flights(
        int[]? marketing = null,
        int[]? operating = null,
        string[]? numbers = null,
        long[]? flights = null,
        int[]? aircraft = null)
        => new(marketing ?? [], operating ?? [], numbers ?? [], flights ?? [], aircraft ?? []);

    public static ProvisionFareApplicationArgs Fares(
        long[]? airFares = null,
        AirFareType[]? types = null,
        long[]? families = null,
        string[]? bases = null,
        int[]? cabins = null,
        long[]? rbds = null)
        => new(airFares ?? [], types ?? [], families ?? [], bases ?? [], cabins ?? [], rbds ?? []);

    public static ProvisionTravelDateArgs Dates(ProvisionDatePeriodArgs[]? permitted = null, ProvisionDatePeriodArgs[]? blackout = null)
        => new(permitted ?? [], blackout ?? []);

    public static ProvisionDayTimeApplicationArgs Windows(params ProvisionDayTimeWindowArgs[] windows) => new(windows);

    public static ProvisionSeatApplicationArgs Seats(string[]? numbers = null, string[]? characteristics = null)
        => new(numbers ?? [], characteristics ?? []);

    public static ProvisionServiceLocationArgs Location(ServiceLocationType type, int id) => new(type, id);
}
