using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed partial class ProvisionEvaluation
    {
        private static readonly TimeSpan EarliestOffset = TimeSpan.FromHours(-12);
        private static readonly TimeSpan LatestOffset = TimeSpan.FromHours(14);

        private void PassengerEligibility()
        {
            if (_provision.PassengerEligibility is not { } rule)
                return;

            if (rule.PassengerTypes.Count > 0 && rule.PassengerTypes.All(row => row.PassengerTypeCode != _unit.Traveller.PassengerTypeCode))
                Outcome.Fail(ShoppingReasonCodes.PassengerTypeNotAllowed);

            if (rule.AgeBands.Count == 0 || Ages() is not { } ages)
                return;

            if (ages.Any(age => !rule.AgeBands.Any(band => age >= band.AgeFromInclusive && (band.AgeToExclusive is null || age < band.AgeToExclusive.Value))))
                Outcome.Fail(ShoppingReasonCodes.AgeOutsideBand);
        }

        private void SalesRestrictions()
        {
            DefinitionSalesDates();

            if (_provision.SalesRestrictions is not { } rule)
                return;

            var now = Context.EvaluatedAtUtc;

            if ((rule.SalesEffectiveFrom is { } from && now < from) || (rule.SalesDiscontinueAt is { } until && now >= until))
                Outcome.Fail(ShoppingReasonCodes.SalesWindowClosed);

            Allow(
                rule.PointsOfSale.Select(row => row.PointOfSaleId),
                (long?)Context.PointOfSaleId,
                ShoppingReasonCodes.PointOfSaleNotAllowed,
                ShoppingReasonCodes.PointOfSaleNotAllowed,
                nameof(Context.PointOfSaleId));
            Allow(
                rule.Customers.Select(row => row.CustomerId),
                Context.CustomerId,
                ShoppingReasonCodes.CustomerNotVerified,
                ShoppingReasonCodes.CustomerNotAllowed,
                nameof(Context.CustomerId));
            Allow(
                rule.CustomerTypes.Select(row => row.CustomerType),
                Context.CustomerType,
                ShoppingReasonCodes.CustomerTypeNotVerified,
                ShoppingReasonCodes.CustomerTypeNotAllowed,
                nameof(Context.CustomerType));
        }

        private void DefinitionSalesDates()
        {
            var earliest = DateOnly.FromDateTime(Context.EvaluatedAtUtc.ToOffset(EarliestOffset).DateTime);
            var latest = DateOnly.FromDateTime(Context.EvaluatedAtUtc.ToOffset(LatestOffset).DateTime);

            if (_definition.SalesEffectiveFrom is { } from)
            {
                if (latest < from)
                    Outcome.Fail(ShoppingReasonCodes.SalesWindowClosed);
                else if (earliest < from)
                    Outcome.Unknown(ShoppingReasonCodes.SalesDateNotDetermined, nameof(AncillaryServiceDefinition.SalesEffectiveFrom));
            }

            if (_definition.SalesDiscontinueOn is { } last)
            {
                if (earliest > last)
                    Outcome.Fail(ShoppingReasonCodes.SalesWindowClosed);
                else if (latest > last)
                    Outcome.Unknown(ShoppingReasonCodes.SalesDateNotDetermined, nameof(AncillaryServiceDefinition.SalesDiscontinueOn));
            }
        }

        private void Geography()
        {
            if (_provision.Geography is not { } rule)
                return;

            foreach (var element in RouteElements())
            {
                Allow(
                    rule.OriginAirports.Select(row => row.AirportId),
                    (int?)element.Origin,
                    ShoppingReasonCodes.OriginNotAllowed,
                    ShoppingReasonCodes.OriginNotAllowed,
                    $"{element.Ref}.OriginAirportId");
                Allow(
                    rule.DestinationAirports.Select(row => row.AirportId),
                    (int?)element.Destination,
                    ShoppingReasonCodes.DestinationNotAllowed,
                    ShoppingReasonCodes.DestinationNotAllowed,
                    $"{element.Ref}.DestinationAirportId");

                if (rule.ViaAirports.Count > 0)
                {
                    if (element.Via is null)
                        Outcome.Unknown(ShoppingReasonCodes.ViaNotVerified, $"{element.Ref}.ViaAirportIds");
                    else if (!element.Via.Any(via => rule.ViaAirports.Any(row => row.AirportId == via)))
                        Outcome.Fail(ShoppingReasonCodes.ViaNotAllowed);
                }

                if (rule.RoutePairs.Count > 0
                    && !rule.RoutePairs.Any(pair => (pair.OriginAirportId == element.Origin && pair.DestinationAirportId == element.Destination)
                                                    || (pair.Direction == RoutePairDirection.BothDirections
                                                        && pair.OriginAirportId == element.Destination
                                                        && pair.DestinationAirportId == element.Origin)))
                    Outcome.Fail(ShoppingReasonCodes.RouteNotAllowed);

                Allow(
                    rule.CoverageCountries.Select(row => row.CountryId),
                    element.DestinationCountry,
                    ShoppingReasonCodes.CountryNotVerified,
                    ShoppingReasonCodes.CountryNotAllowed,
                    $"{element.Ref}.DestinationCountryId");
            }

            if (rule.ServiceLocations.Count == 0)
                return;

            if (_definition.AirportService is not { } airportService)
                Outcome.Unknown(ShoppingReasonCodes.ServiceLocationUnknown, "ServiceLocation");
            else if (!rule.ServiceLocations.Any(row => row.LocationType == ServiceLocationType.Airport && row.LocationId == airportService.AirportId))
            {
                if (rule.ServiceLocations.All(row => row.LocationType == ServiceLocationType.Airport))
                    Outcome.Fail(ShoppingReasonCodes.ServiceLocationNotAllowed);
                else
                    Outcome.Unknown(ShoppingReasonCodes.ServiceLocationUnknown, "ServiceLocation");
            }
        }

        private IEnumerable<(string Ref, int Origin, int Destination, IReadOnlyList<int>? Via, int? DestinationCountry)> RouteElements()
        {
            if (_unit.Scope == ServiceCoverageScope.Sector)
            {
                foreach (var flight in _unit.Flights)
                    yield return ($"Flights[{flight.FlightRef}]", flight.OriginAirportId, flight.DestinationAirportId, flight.ViaAirportIds, flight.DestinationCountryId);

                yield break;
            }

            foreach (var portion in _unit.Portions)
            {
                var last = _unit.Flights.Last(flight => flight.FlightRef == portion.FlightRefs[^1]);

                yield return ($"Portions[{portion.PortionRef}]", portion.OriginAirportId, portion.DestinationAirportId, portion.ViaAirportIds, last.DestinationCountryId);
            }
        }

        private void FlightApplication()
        {
            if (_provision.FlightApplication is not { } rule)
                return;

            foreach (var flight in _unit.Flights)
            {
                var field = $"Flights[{flight.FlightRef}]";

                Allow(
                    rule.MarketingAirlines.Select(row => row.AirlineId),
                    flight.MarketingAirlineId,
                    ShoppingReasonCodes.MarketingAirlineNotVerified,
                    ShoppingReasonCodes.MarketingAirlineNotAllowed,
                    $"{field}.MarketingAirlineId");
                Allow(
                    rule.OperatingAirlines.Select(row => row.AirlineId),
                    flight.OperatingAirlineId,
                    ShoppingReasonCodes.OperatingAirlineNotVerified,
                    ShoppingReasonCodes.OperatingAirlineNotAllowed,
                    $"{field}.OperatingAirlineId");
                AllowText(
                    rule.FlightNumbers.Select(row => row.FlightNumber),
                    flight.FlightNumber,
                    ShoppingReasonCodes.FlightNumberNotVerified,
                    ShoppingReasonCodes.FlightNumberNotAllowed,
                    $"{field}.FlightNumber");
                Allow(
                    rule.Flights.Select(row => row.FlightId),
                    (long?)flight.FlightId,
                    ShoppingReasonCodes.FlightNotAllowed,
                    ShoppingReasonCodes.FlightNotAllowed,
                    $"{field}.FlightId");
                Allow(
                    rule.Aircraft.Select(row => row.AircraftId),
                    flight.AircraftId,
                    ShoppingReasonCodes.AircraftNotVerified,
                    ShoppingReasonCodes.AircraftNotAllowed,
                    $"{field}.AircraftId");
            }
        }

        private void FareApplication()
        {
            if (_provision.FareApplication is not { } rule
                || rule.AirFares.Count + rule.AirFareTypes.Count + rule.FareFamilies.Count + rule.FareBases.Count + rule.CabinClasses.Count + rule.Rbds.Count == 0)
                return;

            foreach (var flight in _unit.Flights)
            {
                var field = $"TravellerFareFacts[{_unit.Traveller.TravellerRef},{flight.FlightRef}]";

                if (_unit.FareOf(flight) is not { } fare)
                {
                    Outcome.Unknown(ShoppingReasonCodes.FareFactsNotVerified, field);

                    continue;
                }

                Allow(rule.AirFares.Select(row => row.AirFareId), fare.AirFareId, ShoppingReasonCodes.FareFactsNotVerified, ShoppingReasonCodes.FareNotAllowed, $"{field}.AirFareId");
                Allow(
                    rule.AirFareTypes.Select(row => row.AirFareType),
                    fare.AirFareType,
                    ShoppingReasonCodes.FareTypeNotVerified,
                    ShoppingReasonCodes.FareTypeNotAllowed,
                    $"{field}.AirFareType");
                Allow(
                    rule.FareFamilies.Select(row => row.FareFamilyId),
                    fare.FareFamilyId,
                    ShoppingReasonCodes.FareFamilyNotVerified,
                    ShoppingReasonCodes.FareFamilyNotAllowed,
                    $"{field}.FareFamilyId");
                AllowText(
                    rule.FareBases.Select(row => row.FareBasisCode),
                    fare.FareBasisCode,
                    ShoppingReasonCodes.FareBasisNotVerified,
                    ShoppingReasonCodes.FareBasisNotAllowed,
                    $"{field}.FareBasisCode");
                Allow(
                    rule.CabinClasses.Select(row => row.CabinClassId),
                    fare.CabinClassId ?? flight.CabinClassId,
                    ShoppingReasonCodes.CabinNotVerified,
                    ShoppingReasonCodes.CabinNotAllowed,
                    $"{field}.CabinClassId");
                Allow(rule.Rbds.Select(row => row.RbdId), fare.RbdId ?? flight.RbdId, ShoppingReasonCodes.RbdNotVerified, ShoppingReasonCodes.RbdNotAllowed, $"{field}.RbdId");
            }
        }

        private void TravelDate()
        {
            if (_provision.TravelDate is not { } rule || (rule.PermittedPeriods.Count == 0 && rule.BlackoutPeriods.Count == 0) || Occurrences() is not { } occurrences)
                return;

            foreach (var date in occurrences.Select(occurrence => occurrence.LocalDate))
            {
                if (rule.BlackoutPeriods.Any(period => period.StartDate <= date && date <= period.EndDate))
                    Outcome.Fail(ShoppingReasonCodes.TravelDateBlackout);
                else if (rule.PermittedPeriods.Count > 0 && !rule.PermittedPeriods.Any(period => period.StartDate <= date && date <= period.EndDate))
                    Outcome.Fail(ShoppingReasonCodes.TravelDateNotPermitted);
            }
        }

        private void DayTimeApplication()
        {
            if (_provision.DayTimeApplication is not { } rule || rule.Windows.Count == 0 || Occurrences() is not { } occurrences)
                return;

            foreach (var occurrence in occurrences)
            {
                var day = (byte)(1 << (((int)occurrence.Local.DayOfWeek + 6) % 7));
                var time = TimeOnly.FromDateTime(occurrence.Local);
                var matching = rule.Windows
                    .Where(window => (window.DaysOfWeekMask & day) != 0
                                     && (window.StartLocalTime is null || time >= window.StartLocalTime.Value)
                                     && (window.EndLocalTime is null || time < window.EndLocalTime.Value))
                    .ToList();

                if (matching.Any(window => window.Effect == DayTimeRestrictionEffect.Deny))
                    Outcome.Fail(ShoppingReasonCodes.DayTimeDenied);
                else if (rule.Windows.Any(window => window.Effect == DayTimeRestrictionEffect.Allow) && matching.All(window => window.Effect != DayTimeRestrictionEffect.Allow))
                    Outcome.Fail(ShoppingReasonCodes.DayTimeNotAllowed);
            }
        }

        private void AdvancePurchase()
        {
            if (_provision.AdvancePurchase is not { } rule)
                return;

            var sale = Context.EvaluatedAtUtc;

            if (rule.SameTimeAsTicketed)
            {
                if (Context.SourceIdentity.TicketedAtUtc is not { } ticketedAt)
                    Outcome.Unknown(ShoppingReasonCodes.TicketTimeNotVerified, nameof(Context.SourceIdentity.TicketedAtUtc));
                else if (ticketedAt != sale)
                    Outcome.Fail(ShoppingReasonCodes.NotSoldWithTicket);
            }

            if ((rule.MinimumPeriod == 0 && rule.MaximumPeriod is null) || Occurrences() is not { } occurrences)
                return;

            foreach (var occurrence in occurrences)
            {
                var saleDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(sale, occurrence.Zone).DateTime);
                var elapsed = occurrence.Instant - sale;
                (bool Early, bool Late)? lead = rule.Unit switch
                {
                    TimeUnit.Minutes => (
                        rule.MaximumPeriod is { } maximum && elapsed > TimeSpan.FromMinutes(maximum),
                        elapsed < TimeSpan.FromMinutes(rule.MinimumPeriod)),
                    TimeUnit.Hours => (
                        rule.MaximumPeriod is { } maximum && elapsed > TimeSpan.FromHours(maximum),
                        elapsed < TimeSpan.FromHours(rule.MinimumPeriod)),
                    TimeUnit.Days => (
                        rule.MaximumPeriod is { } maximum && saleDate.AddDays(maximum) < occurrence.LocalDate,
                        saleDate.AddDays(rule.MinimumPeriod) > occurrence.LocalDate),
                    TimeUnit.Months => (
                        rule.MaximumPeriod is { } maximum && saleDate.AddMonths(maximum) < occurrence.LocalDate,
                        saleDate.AddMonths(rule.MinimumPeriod) > occurrence.LocalDate),
                    _ => null
                };

                if (lead is null)
                    Outcome.Unknown(ShoppingReasonCodes.AdvancePurchaseUnitNotSupported, nameof(rule.Unit));
                else if (lead.Value.Late)
                    Outcome.Fail(ShoppingReasonCodes.AdvancePurchaseTooLate);
                else if (lead.Value.Early)
                    Outcome.Fail(ShoppingReasonCodes.AdvancePurchaseTooEarly);
            }
        }

        private void BaggageApplication()
        {
            if (_definition.Baggage is null)
                return;

            var rule = _provision.BaggageApplication;
            var cabin = _definition.VariantCode == AncillaryVariant.CabinBag;

            if (_definition.VariantCode is AncillaryVariant.ExtraCheckedBag or AncillaryVariant.CabinBag || rule?.FreePieces is not null)
            {
                foreach (var flight in _unit.Flights)
                {
                    var facts = _unit.BaggageOf(flight);
                    var pieces = cabin ? facts?.CabinPieces : facts?.CheckedPieces;

                    if (facts is not { SourceCompleteness: FactEvidence.Verified } || pieces is null)
                        Outcome.Unknown(ShoppingReasonCodes.BaggageAllowanceNotVerified, $"TravellerBaggageFacts[{_unit.Traveller.TravellerRef},{flight.FlightRef}]");
                    else if (rule?.FreePieces is { } free && pieces.Value != free)
                        Outcome.Fail(ShoppingReasonCodes.BaggageAllowanceMismatch);
                }
            }

            if (rule is null)
                return;

            if (rule.PurchaseApplication == BaggagePurchaseApplication.CheckIn)
                Outcome.Fail(ShoppingReasonCodes.BaggageCheckInPurchaseOnly);

            if (rule.FirstExcessPiece is null && rule.LastExcessPiece is null)
                return;

            if (!Context.CoverageCompleteness.ExistingServicesComplete)
            {
                Outcome.Unknown(ShoppingReasonCodes.ExistingServicesNotVerified, nameof(Context.ExistingServiceFacts));

                return;
            }

            var owned = _unit.ActiveServices().Where(service => service.ServiceRef == _definition.ServiceDefinitionRef).Sum(service => service.Quantity);
            var first = owned + 1;
            var last = owned + Math.Max(Selection?.Quantity ?? 1, 1);

            if (first < (rule.FirstExcessPiece ?? 1) || last > (rule.LastExcessPiece ?? int.MaxValue))
                Outcome.Fail(ShoppingReasonCodes.BaggagePieceOutsideTier);
        }

        private void SeatApplication()
        {
            if (_provision.SeatApplication is not { } rule || rule.SeatNumbers.Count + rule.SeatCharacteristics.Count == 0)
                return;

            if (Selection?.SeatNumber is not { } seatNumber)
            {
                Outcome.AwaitSelection(ShoppingReasonCodes.SeatSelectionPending, "Selection.SeatNumber");

                return;
            }

            if (rule.SeatNumbers.Count > 0 && !rule.SeatNumbers.Any(row => string.Equals(row.SeatNumber, seatNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
                Outcome.Reject("SeatNumber", ShoppingReasonCodes.SeatNumberNotAllowed);

            if (rule.SeatCharacteristics.Count == 0)
                return;

            if (Selection.VerifiedSeatCharacteristicCodes is not { } characteristics)
                Outcome.Unknown(ShoppingReasonCodes.SeatMapNotVerified, "Selection.VerifiedSeatCharacteristicCodes");
            else if (!characteristics.Any(code => rule.SeatCharacteristics.Any(row => string.Equals(row.CharacteristicCode, code, StringComparison.OrdinalIgnoreCase))))
                Outcome.Reject("SeatNumber", ShoppingReasonCodes.SeatCharacteristicNotAllowed);
        }
    }
}
