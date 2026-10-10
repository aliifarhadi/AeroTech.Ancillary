using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class ShoppingContextValidator
    {
        public static void Validate(AncillaryShoppingContext context, ShoppingFilter filter)
        {
            Require(context.ContextSchemaVersion == AncillaryShoppingContext.CurrentSchemaVersion, nameof(context.ContextSchemaVersion));
            Require(context.OwnerAirlineId > 0, nameof(context.OwnerAirlineId));
            Require(context.PointOfSaleId > 0, nameof(context.PointOfSaleId));
            Require(context.CurrencyId > 0, nameof(context.CurrencyId));
            Require(context.CustomerId is null or > 0, nameof(context.CustomerId));
            Require(context.CustomerType is null || Enum.IsDefined(context.CustomerType.Value), nameof(context.CustomerType));
            Require(Enum.IsDefined(context.ShoppingStage), nameof(context.ShoppingStage));
            Require(context.EvaluatedAtUtc.Offset == TimeSpan.Zero, nameof(context.EvaluatedAtUtc));
            Require(context.CoverageCompleteness is not null, nameof(context.CoverageCompleteness));
            Source(context.SourceIdentity);
            Travellers(context);
            Itinerary(context);
            Facts(context);
            Filter(context, filter);
        }

        private static void Source(SourceIdentityContext source)
        {
            Require(source is not null && Enum.IsDefined(source.SourceKind), nameof(SourceIdentityContext.SourceKind));
            Require(source!.SourceKind == ShoppingSourceKind.Order ? source.OrderId is > 0 : source.OrderId is null, nameof(SourceIdentityContext.OrderId));
            Require(source.TicketedAtUtc is null || source.TicketedAtUtc.Value.Offset == TimeSpan.Zero, nameof(SourceIdentityContext.TicketedAtUtc));
            Require(source.ValidUntilUtc is null || source.ValidUntilUtc.Value.Offset == TimeSpan.Zero, nameof(SourceIdentityContext.ValidUntilUtc));
        }

        private static void Travellers(AncillaryShoppingContext context)
        {
            Require(context.Travellers is { Count: > 0 }, nameof(context.Travellers));
            Unique(context.Travellers.Select(traveller => traveller.TravellerRef), nameof(ShoppingTraveller.TravellerRef));

            foreach (var traveller in context.Travellers)
            {
                Require(Enum.IsDefined(traveller.PassengerTypeCode), nameof(ShoppingTraveller.PassengerTypeCode));
                Require(traveller.OrderTravellerId is null or > 0, nameof(ShoppingTraveller.OrderTravellerId));
                Require(traveller.VerifiedAgeAtTravel is null or >= 0, nameof(ShoppingTraveller.VerifiedAgeAtTravel));
                Require(traveller.VerifiedAgeAtTravel is null || traveller.AgeEvidenceAsOfDate is not null, nameof(ShoppingTraveller.AgeEvidenceAsOfDate));
                Require(
                    traveller.AssociatedAdultRef is null
                    || (traveller.AssociatedAdultRef != traveller.TravellerRef && context.Travellers.Any(adult => adult.TravellerRef == traveller.AssociatedAdultRef)),
                    nameof(ShoppingTraveller.AssociatedAdultRef));
            }
        }

        private static void Itinerary(AncillaryShoppingContext context)
        {
            Require(context.Flights is { Count: > 0 }, nameof(context.Flights));
            Require(context.Portions is { Count: > 0 }, nameof(context.Portions));
            Unique(context.Flights.Select(flight => flight.FlightRef), nameof(ShoppingFlight.FlightRef));
            Unique(context.Portions.Select(portion => portion.PortionRef), nameof(ShoppingPortion.PortionRef));
            Require(context.Portions.Select(portion => portion.Sequence).Distinct().Count() == context.Portions.Count, nameof(ShoppingPortion.Sequence));

            foreach (var flight in context.Flights)
            {
                Require(flight.FlightId > 0, nameof(ShoppingFlight.FlightId));
                Require(flight.OriginAirportId > 0 && flight.DestinationAirportId > 0, nameof(ShoppingFlight.OriginAirportId));
                Require(flight.DepartureAt < flight.ArrivalAt, nameof(ShoppingFlight.ArrivalAt));
                Require(context.Portions.Count(portion => portion.FlightRefs.Contains(flight.FlightRef, StringComparer.Ordinal)) == 1, nameof(ShoppingPortion.FlightRefs));
            }

            foreach (var portion in context.Portions)
            {
                Require(portion.Sequence > 0, nameof(ShoppingPortion.Sequence));
                Require(portion.OriginAirportId > 0 && portion.DestinationAirportId > 0, nameof(ShoppingPortion.OriginAirportId));
                Require(portion.FlightRefs is { Count: > 0 }, nameof(ShoppingPortion.FlightRefs));
                Require(portion.FlightRefs.Distinct(StringComparer.Ordinal).Count() == portion.FlightRefs.Count, nameof(ShoppingPortion.FlightRefs));
                Require(portion.FlightRefs.All(flightRef => context.Flights.Any(flight => flight.FlightRef == flightRef)), nameof(ShoppingPortion.FlightRefs));
            }

            var first = context.Flights.Min(flight => flight.DepartureAt);

            Require(
                context.Travellers.All(traveller => traveller.DateOfBirth is null || traveller.DateOfBirth.Value <= DateOnly.FromDateTime(first.UtcDateTime.AddDays(1))),
                nameof(ShoppingTraveller.DateOfBirth));
        }

        private static void Facts(AncillaryShoppingContext context)
        {
            bool Known(string travellerRef, string flightRef)
                => context.Travellers.Any(traveller => traveller.TravellerRef == travellerRef) && context.Flights.Any(flight => flight.FlightRef == flightRef);

            Require(context.TravellerFareFacts.All(fact => Known(fact.TravellerRef, fact.FlightRef)), nameof(context.TravellerFareFacts));
            Require(
                context.TravellerFareFacts.GroupBy(fact => (fact.TravellerRef, fact.FlightRef)).All(group => group.Count() == 1),
                nameof(context.TravellerFareFacts));
            Require(context.TravellerBaggageFacts.All(fact => Known(fact.TravellerRef, fact.FlightRef)), nameof(context.TravellerBaggageFacts));
            Require(
                context.TravellerBaggageFacts.GroupBy(fact => (fact.TravellerRef, fact.FlightRef)).All(group => group.Count() == 1),
                nameof(context.TravellerBaggageFacts));
            Require(
                context.FareEntitlementFacts.All(fact => !string.IsNullOrWhiteSpace(fact.BenefitCode) && fact.FlightRefs.All(flightRef => Known(fact.TravellerRef, flightRef))),
                nameof(context.FareEntitlementFacts));
            Require(
                context.ExistingServiceFacts.All(fact => fact.Quantity >= 0
                                                         && !string.IsNullOrWhiteSpace(fact.ServiceRef)
                                                         && fact.FlightRefs.All(flightRef => Known(fact.TravellerRef, flightRef))),
                nameof(context.ExistingServiceFacts));
            Require(
                context.UsageEvidence.All(evidence => evidence.UnitsConsumed >= 0 && !string.IsNullOrWhiteSpace(evidence.SourceAuthority)),
                nameof(context.UsageEvidence));
        }

        private static void Filter(AncillaryShoppingContext context, ShoppingFilter filter)
        {
            Require(filter.Profiles is null || filter.Profiles.All(profile => Enum.IsDefined(profile)), nameof(filter.Profiles));
            Require(filter.VariantCodes is null || filter.VariantCodes.All(code => AncillaryVariant.Find(code) is not null), nameof(filter.VariantCodes));
            Require(
                filter.FlightRefs is null || filter.FlightRefs.All(flightRef => context.Flights.Any(flight => flight.FlightRef == flightRef)),
                nameof(filter.FlightRefs));
            Require(
                filter.TravellerRefs is null || filter.TravellerRefs.All(travellerRef => context.Travellers.Any(traveller => traveller.TravellerRef == travellerRef)),
                nameof(filter.TravellerRefs));
        }

        private static void Unique(IEnumerable<string> references, string field)
        {
            var values = references.ToList();

            Require(values.All(value => !string.IsNullOrWhiteSpace(value)) && values.Distinct(StringComparer.Ordinal).Count() == values.Count, field);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ShoppingContextIsInvalid(field);
        }
    }
}
