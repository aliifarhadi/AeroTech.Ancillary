using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Oracle;

public static class ProvisionRuleOracle
{
    public static OracleDecision Select(IEnumerable<AncillaryProvision> provisions, OracleContext context)
    {
        foreach (var provision in provisions.Where(candidate => candidate.Status == ProvisionStatus.Active).OrderBy(candidate => candidate.Sequence))
        {
            var verdict = Evaluate(provision, context);

            if (verdict == OracleVerdict.UnsupportedContext)
                return new OracleDecision(OracleOutcome.UnsupportedContext, provision.Id);

            if (verdict == OracleVerdict.Match)
                return new OracleDecision(
                    provision.Outcome.Disposition switch
                    {
                        CommercialDisposition.Paid => OracleOutcome.Paid,
                        CommercialDisposition.Free => OracleOutcome.Free,
                        _ => OracleOutcome.NotAvailable
                    },
                    provision.Id);
        }

        return new OracleDecision(OracleOutcome.NoMatch, null);
    }

    public static OracleVerdict Evaluate(AncillaryProvision provision, OracleContext context)
        => Combine(
        [
            .. PassengerEligibility(provision.PassengerEligibility, context),
            .. SalesRestrictions(provision.SalesRestrictions, context),
            .. Geography(provision.Geography, context),
            .. FlightApplication(provision.FlightApplication, context),
            .. FareApplication(provision.FareApplication, context),
            TravelDate(provision.TravelDate, context),
            DayTimeApplication(provision.DayTimeApplication, context),
            AdvancePurchase(provision.AdvancePurchase, context),
            .. SeatApplication(provision.SeatApplication, context)
        ]);

    public static OracleVerdict TravelDate(ProvisionTravelDateRule? rule, OracleContext context)
    {
        if (rule is null || (rule.PermittedPeriods.Count == 0 && rule.BlackoutPeriods.Count == 0))
            return OracleVerdict.Match;

        if (context.Occurrence is null || !context.Occurrence.TryResolve(out var local, out _))
            return OracleVerdict.UnsupportedContext;

        var date = DateOnly.FromDateTime(local);

        if (rule.BlackoutPeriods.Any(period => period.StartDate <= date && date <= period.EndDate))
            return OracleVerdict.NoMatch;

        return rule.PermittedPeriods.Count == 0 || rule.PermittedPeriods.Any(period => period.StartDate <= date && date <= period.EndDate)
            ? OracleVerdict.Match
            : OracleVerdict.NoMatch;
    }

    public static OracleVerdict DayTimeApplication(ProvisionDayTimeApplicationRule? rule, OracleContext context)
    {
        if (rule is null || rule.Windows.Count == 0)
            return OracleVerdict.Match;

        if (context.Occurrence is null || !context.Occurrence.TryResolve(out var local, out _))
            return OracleVerdict.UnsupportedContext;

        var day = (byte)(1 << (((int)local.DayOfWeek + 6) % 7));
        var time = TimeOnly.FromDateTime(local);
        var matching = rule.Windows
            .Where(window => (window.DaysOfWeekMask & day) != 0
                && (window.StartLocalTime is null || time >= window.StartLocalTime.Value)
                && (window.EndLocalTime is null || time < window.EndLocalTime.Value))
            .ToList();

        if (matching.Any(window => window.Effect == DayTimeRestrictionEffect.Deny))
            return OracleVerdict.NoMatch;

        return rule.Windows.All(window => window.Effect != DayTimeRestrictionEffect.Allow) || matching.Any(window => window.Effect == DayTimeRestrictionEffect.Allow)
            ? OracleVerdict.Match
            : OracleVerdict.NoMatch;
    }

    public static OracleVerdict AdvancePurchase(ProvisionAdvancePurchaseRule? rule, OracleContext context)
    {
        if (rule is null)
            return OracleVerdict.Match;

        if (context.SaleInstant is null)
            return OracleVerdict.UnsupportedContext;

        if (rule.SameTimeAsTicketed)
        {
            if (context.TicketedAt is null)
                return OracleVerdict.UnsupportedContext;

            if (context.TicketedAt.Value != context.SaleInstant.Value)
                return OracleVerdict.NoMatch;
        }

        if (rule.MinimumPeriod == 0)
            return OracleVerdict.Match;

        if (context.Occurrence is null || !context.Occurrence.TryResolve(out var local, out var instant))
            return OracleVerdict.UnsupportedContext;

        var saleLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(context.SaleInstant.Value, context.Occurrence.Zone!).DateTime);
        var occurrenceDate = DateOnly.FromDateTime(local);
        bool? satisfied = rule.Unit switch
        {
            TimeUnit.Minutes => instant - context.SaleInstant.Value >= TimeSpan.FromMinutes(rule.MinimumPeriod),
            TimeUnit.Hours => instant - context.SaleInstant.Value >= TimeSpan.FromHours(rule.MinimumPeriod),
            TimeUnit.Days => saleLocalDate.AddDays(rule.MinimumPeriod) <= occurrenceDate,
            TimeUnit.Months => saleLocalDate.AddMonths(rule.MinimumPeriod) <= occurrenceDate,
            _ => null
        };

        return satisfied switch
        {
            true => OracleVerdict.Match,
            false => OracleVerdict.NoMatch,
            _ => OracleVerdict.UnsupportedContext
        };
    }

    public static IReadOnlyList<AncillaryPricingRate> SelectedRates(AncillaryPricing pricing, PassengerTypeCode? passengerType, int? age)
        => pricing.Rates
            .Where(rate => rate.PassengerTypeCode is null || rate.PassengerTypeCode == passengerType)
            .Where(rate => rate.AgeFromInclusive is null || (age is not null && age.Value >= rate.AgeFromInclusive.Value && (rate.AgeToExclusive is null || age.Value < rate.AgeToExclusive.Value)))
            .ToList();

    public static decimal? UnitTotal(AncillaryPricing pricing, PassengerTypeCode? passengerType, int? age)
    {
        var rates = SelectedRates(pricing, passengerType, age);

        return rates.Count == 1 ? rates[0].UnitTotal.Amount : null;
    }

    public static OracleRate SelectRate(AncillaryPricing pricing, int currencyId, PassengerTypeCode? passengerType, int? age)
    {
        if (pricing.Rates.All(rate => rate.CurrencyId != currencyId))
            return new OracleRate(OracleRateOutcome.NoMatchingCurrency, null);

        var rates = SelectedRates(pricing, passengerType, age).Where(rate => rate.CurrencyId == currencyId).ToList();

        return rates.Count == 1 ? new OracleRate(OracleRateOutcome.Selected, rates[0]) : new OracleRate(OracleRateOutcome.NoMatchingSelector, null);
    }

    public static int CompletedYears(DateOnly dateOfBirth, DateOnly at)
    {
        var years = at.Year - dateOfBirth.Year;

        return dateOfBirth.AddYears(years) > at ? years - 1 : years;
    }

    private static IEnumerable<OracleVerdict> PassengerEligibility(ProvisionPassengerEligibilityRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        yield return AllowList(rule.PassengerTypes.Select(row => row.PassengerTypeCode), context.PassengerType);

        if (rule.AgeBands.Count == 0)
            yield break;

        if (context.DateOfBirth is null || context.Occurrence is null || !context.Occurrence.TryResolve(out var local, out _))
        {
            yield return OracleVerdict.UnsupportedContext;

            yield break;
        }

        var age = CompletedYears(context.DateOfBirth.Value, DateOnly.FromDateTime(local));

        yield return rule.AgeBands.Any(band => age >= band.AgeFromInclusive && (band.AgeToExclusive is null || age < band.AgeToExclusive.Value))
            ? OracleVerdict.Match
            : OracleVerdict.NoMatch;
    }

    private static IEnumerable<OracleVerdict> SalesRestrictions(ProvisionSalesRestrictionsRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        if (rule.SalesEffectiveFrom is not null || rule.SalesDiscontinueAt is not null)
            yield return context.SaleInstant is null
                ? OracleVerdict.UnsupportedContext
                : (rule.SalesEffectiveFrom is null || context.SaleInstant.Value >= rule.SalesEffectiveFrom.Value)
                    && (rule.SalesDiscontinueAt is null || context.SaleInstant.Value < rule.SalesDiscontinueAt.Value)
                    ? OracleVerdict.Match
                    : OracleVerdict.NoMatch;

        yield return AllowList(rule.PointsOfSale.Select(row => row.PointOfSaleId), context.PointOfSaleId);
        yield return AllowList(rule.Customers.Select(row => row.CustomerId), context.CustomerId);
        yield return AllowList(rule.CustomerTypes.Select(row => row.CustomerType), context.CustomerType);
    }

    private static IEnumerable<OracleVerdict> Geography(ProvisionGeographyRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        yield return AllowList(rule.OriginAirports.Select(row => row.AirportId), context.OriginAirportId);
        yield return AllowList(rule.DestinationAirports.Select(row => row.AirportId), context.DestinationAirportId);
        yield return AnyOf(rule.ViaAirports.Select(row => row.AirportId), context.ViaAirportIds);

        if (rule.RoutePairs.Count > 0)
            yield return context.OriginAirportId is null || context.DestinationAirportId is null
                ? OracleVerdict.UnsupportedContext
                : rule.RoutePairs.Any(pair =>
                    (pair.OriginAirportId == context.OriginAirportId && pair.DestinationAirportId == context.DestinationAirportId)
                    || (pair.Direction == RoutePairDirection.BothDirections && pair.OriginAirportId == context.DestinationAirportId && pair.DestinationAirportId == context.OriginAirportId))
                    ? OracleVerdict.Match
                    : OracleVerdict.NoMatch;

        yield return AnyOf(rule.ServiceLocations.Select(row => new OracleServicePlace(row.LocationType, row.LocationId)), context.ServicePlaces);
        yield return AllowList(rule.CoverageCountries.Select(row => row.CountryId), context.CoverageCountryId);
    }

    private static IEnumerable<OracleVerdict> FlightApplication(ProvisionFlightApplicationRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        yield return AllowList(rule.MarketingAirlines.Select(row => row.AirlineId), context.MarketingAirlineId);
        yield return AllowList(rule.OperatingAirlines.Select(row => row.AirlineId), context.OperatingAirlineId);
        yield return AllowText(rule.FlightNumbers.Select(row => row.FlightNumber), context.FlightNumber);
        yield return AllowList(rule.Flights.Select(row => row.FlightId), context.FlightId);
        yield return AllowList(rule.Aircraft.Select(row => row.AircraftId), context.AircraftId);
    }

    private static IEnumerable<OracleVerdict> FareApplication(ProvisionFareApplicationRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        yield return AllowList(rule.AirFares.Select(row => row.AirFareId), context.AirFareId);
        yield return AllowList(rule.AirFareTypes.Select(row => row.AirFareType), context.AirFareType);
        yield return AllowList(rule.FareFamilies.Select(row => row.FareFamilyId), context.FareFamilyId);
        yield return AllowText(rule.FareBases.Select(row => row.FareBasisCode), context.FareBasisCode);
        yield return AllowList(rule.CabinClasses.Select(row => row.CabinClassId), context.CabinClassId);
        yield return AllowList(rule.Rbds.Select(row => row.RbdId), context.RbdId);
    }

    private static IEnumerable<OracleVerdict> SeatApplication(ProvisionSeatApplicationRule? rule, OracleContext context)
    {
        if (rule is null)
            yield break;

        yield return AllowText(rule.SeatNumbers.Select(row => row.SeatNumber), context.SeatNumber);
        yield return AnyOf(rule.SeatCharacteristics.Select(row => row.CharacteristicCode), context.SeatCharacteristicCodes);
    }

    private static OracleVerdict Combine(IReadOnlyList<OracleVerdict> verdicts)
        => verdicts.Contains(OracleVerdict.UnsupportedContext)
            ? OracleVerdict.UnsupportedContext
            : verdicts.Contains(OracleVerdict.NoMatch) ? OracleVerdict.NoMatch : OracleVerdict.Match;

    private static OracleVerdict AllowList<TValue>(IEnumerable<TValue> allowed, TValue? actual)
        where TValue : struct
    {
        var values = allowed.ToList();

        if (values.Count == 0)
            return OracleVerdict.Match;

        if (actual is null)
            return OracleVerdict.UnsupportedContext;

        return values.Contains(actual.Value) ? OracleVerdict.Match : OracleVerdict.NoMatch;
    }

    private static OracleVerdict AllowText(IEnumerable<string> allowed, string? actual)
    {
        var values = allowed.ToList();

        if (values.Count == 0)
            return OracleVerdict.Match;

        if (string.IsNullOrWhiteSpace(actual))
            return OracleVerdict.UnsupportedContext;

        return values.Contains(actual.Trim().ToUpperInvariant(), StringComparer.Ordinal) ? OracleVerdict.Match : OracleVerdict.NoMatch;
    }

    private static OracleVerdict AnyOf<TValue>(IEnumerable<TValue> allowed, IReadOnlyList<TValue>? actual)
    {
        var values = allowed.ToList();

        if (values.Count == 0)
            return OracleVerdict.Match;

        if (actual is null)
            return OracleVerdict.UnsupportedContext;

        return actual.Any(values.Contains) ? OracleVerdict.Match : OracleVerdict.NoMatch;
    }
}
