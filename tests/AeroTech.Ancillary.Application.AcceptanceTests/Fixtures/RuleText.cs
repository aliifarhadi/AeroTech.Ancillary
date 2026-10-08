using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class RuleText
{
    public static string Of(BackofficeProvisionDto provision)
    {
        var parts = new List<string>();

        void Add<TValue>(string label, IEnumerable<TValue>? values)
        {
            var list = values?.Select(value => Convert.ToString(value, CultureInfo.InvariantCulture)).ToList();

            if (list is { Count: > 0 })
                parts.Add($"{label}={string.Join(",", list)}");
        }

        string Period(BackofficeProvisionDatePeriodDto period) => $"{period.StartDate:yyyy-MM-dd}..{period.EndDate:yyyy-MM-dd}";

        Add("PTC", provision.PassengerEligibility?.AllowedPassengerTypes.Select(row => row.Value.Name));
        Add("AGE", provision.PassengerEligibility?.AllowedAgeBands.Select(row => $"{row.AgeFromInclusive}-{row.AgeToExclusive}"));

        if (provision.SalesRestrictions?.SalesEffectiveFrom is { } salesFrom)
            parts.Add($"SALEFROM={salesFrom.UtcDateTime:yyyy-MM-dd}");

        if (provision.SalesRestrictions?.SalesDiscontinueAt is { } salesUntil)
            parts.Add($"SALEUNTIL={salesUntil.UtcDateTime:yyyy-MM-dd}");

        Add("POS", provision.SalesRestrictions?.AllowedPointsOfSale.Select(row => row.Value));
        Add("CUST", provision.SalesRestrictions?.AllowedCustomers.Select(row => row.Value));
        Add("CUSTTYPE", provision.SalesRestrictions?.AllowedCustomerTypes.Select(row => row.Value.Name));
        Add("ORG", provision.Geography?.AllowedOriginAirports.Select(row => row.Value));
        Add("DST", provision.Geography?.AllowedDestinationAirports.Select(row => row.Value));
        Add("VIA", provision.Geography?.AllowedViaAirports.Select(row => row.Value));
        Add("ROUTE", provision.Geography?.AllowedRoutePairs.Select(row => $"{row.OriginAirportId}{(row.Direction.Name == "BothDirections" ? "<>" : ">")}{row.DestinationAirportId}"));
        Add("AT", provision.Geography?.ServiceLocations.Select(row => $"{row.LocationType.Name}:{row.LocationId}"));
        Add("COVER", provision.Geography?.CoverageCountries.Select(row => row.Value));
        Add("MKT", provision.FlightApplication?.AllowedMarketingAirlines.Select(row => row.Value));
        Add("OPR", provision.FlightApplication?.AllowedOperatingAirlines.Select(row => row.Value));
        Add("FLTNO", provision.FlightApplication?.AllowedFlightNumbers.Select(row => row.Value));
        Add("FLT", provision.FlightApplication?.AllowedFlights.Select(row => row.Value));
        Add("ACFT", provision.FlightApplication?.AllowedAircraft.Select(row => row.Value));
        Add("FARE", provision.FareApplication?.AllowedAirFares.Select(row => row.Value));
        Add("FARETYPE", provision.FareApplication?.AllowedAirFareTypes.Select(row => row.Value.Name));
        Add("FAMILY", provision.FareApplication?.AllowedFareFamilies.Select(row => row.Value));
        Add("BASIS", provision.FareApplication?.AllowedFareBases.Select(row => row.Value));
        Add("CABIN", provision.FareApplication?.AllowedCabinClasses.Select(row => row.Value));
        Add("RBD", provision.FareApplication?.AllowedRbds.Select(row => row.Value));
        Add("PERMIT", provision.TravelDate?.PermittedPeriods.Select(Period));
        Add("BLACKOUT", provision.TravelDate?.BlackoutPeriods.Select(Period));
        Add("TIME", provision.DayTimeApplication?.Windows.Select(row => $"{row.DaysOfWeekMask}:{row.StartLocalTime?.Hour}-{row.EndLocalTime?.Hour}:{row.Effect.Name}"));

        if (provision.AdvancePurchase is { } advance)
            parts.Add($"ADVANCE={advance.MinimumPeriod}{advance.Unit.Name}{(advance.SameTimeAsTicketed ? "+TICKET" : string.Empty)}");

        if (provision.BaggageApplication is { } baggage)
            parts.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"BAG={baggage.FirstExcessPiece}-{baggage.LastExcessPiece}:{baggage.Weight:0.##}{baggage.WeightUnit.Name}:{baggage.PurchaseApplication.Name}"));

        Add("SEAT", provision.SeatApplication?.SeatNumbers.Select(row => row.Value));
        Add("SEATCHAR", provision.SeatApplication?.SeatCharacteristics.Select(row => row.Value));

        return string.Join("; ", parts);
    }

    public static string Of(BackofficePricingDto? pricing)
        => pricing is null
            ? "-"
            : string.Create(
                CultureInfo.InvariantCulture,
                $"{pricing.PricingUnit?.Name} {pricing.Currency} {string.Join(" | ", pricing.Rates.Select(rate => string.Create(CultureInfo.InvariantCulture, $"{rate.PassengerTypeCode?.Name ?? "*"}[{rate.AgeFromInclusive}-{rate.AgeToExclusive}]={rate.BaseAmount:0.##}+{rate.TaxAmount:0.##}+{rate.FeeAmount:0.##}={rate.TotalAmount:0.##}")))}");
}
