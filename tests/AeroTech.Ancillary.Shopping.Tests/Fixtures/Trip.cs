using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class Trip
{
    public const int Thr = 1;
    public const int Ika = 2;
    public const int Mhd = 3;
    public const int Ist = 6;
    public const string Tehran = "Asia/Tehran";
    public const string Istanbul = "Europe/Istanbul";
    public const string Paris = "Europe/Paris";

    public static readonly TimeSpan TehranOffset = new(3, 30, 0);

    public static ShoppingTraveller Adult(string reference = "T1")
        => new()
        {
            TravellerRef = reference,
            PassengerTypeCode = PassengerTypeCode.ADT,
            DateOfBirth = new DateOnly(1990, 5, 20),
            StableTravellerIdentity = $"PAX-{reference}",
            VerifiedExitRowEligible = true
        };

    public static ShoppingTraveller Child(string reference = "T2", DateOnly? dateOfBirth = null)
        => new()
        {
            TravellerRef = reference,
            PassengerTypeCode = PassengerTypeCode.CHD,
            DateOfBirth = dateOfBirth ?? new DateOnly(2018, 6, 15),
            StableTravellerIdentity = $"PAX-{reference}",
            VerifiedExitRowEligible = false
        };

    public static ShoppingFlight Flight(
        string reference = "F1",
        long flightId = 101,
        int origin = Thr,
        int destination = Ist,
        DateTimeOffset? departure = null,
        string? zone = Tehran)
    {
        var departs = departure ?? new DateTimeOffset(2026, 12, 1, 10, 0, 0, TehranOffset);

        return new ShoppingFlight
        {
            FlightRef = reference,
            FlightId = flightId,
            FlightNumber = "111",
            MarketingAirlineId = 1,
            OperatingAirlineId = 1,
            OriginAirportId = origin,
            DestinationAirportId = destination,
            ViaAirportIds = [],
            OriginCountryId = 100,
            DestinationCountryId = 200,
            DepartureAt = departs,
            ArrivalAt = departs.AddHours(3),
            OriginIanaTimeZoneId = zone,
            AircraftId = 1,
            CabinClassId = 1,
            RbdId = 11
        };
    }

    public static ShoppingPortion Portion(string reference, int sequence, params ShoppingFlight[] flights)
        => new()
        {
            PortionRef = reference,
            Sequence = sequence,
            FlightRefs = flights.Select(flight => flight.FlightRef).ToList(),
            OriginAirportId = flights[0].OriginAirportId,
            DestinationAirportId = flights[^1].DestinationAirportId,
            ViaAirportIds = flights.SkipLast(1).Select(flight => flight.DestinationAirportId).ToList()
        };

    public static AncillaryShoppingContext Context(
        IReadOnlyList<ShoppingFlight>? flights = null,
        IReadOnlyList<ShoppingPortion>? portions = null,
        IReadOnlyList<ShoppingTraveller>? travellers = null,
        ShoppingStage stage = ShoppingStage.PreOrder,
        long pointOfSaleId = ShoppingLab.Pos,
        int currencyId = ShoppingLab.Eur)
    {
        var legs = flights ?? [Flight()];
        var people = travellers ?? [Adult()];
        var bounds = portions ?? [Portion("P1", 1, legs.ToArray())];

        return new AncillaryShoppingContext
        {
            OwnerAirlineId = ShoppingLab.Airline,
            PointOfSaleId = pointOfSaleId,
            ShoppingStage = stage,
            EvaluatedAtUtc = ShoppingLab.Now,
            CurrencyId = currencyId,
            SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Offer, TrustedSourceReference = "OFFER-1", TrustedSourceVersion = "v1" },
            Travellers = people,
            Portions = bounds,
            Flights = legs,
            TravellerFareFacts = people.SelectMany(person => bounds.SelectMany(bound => bound.FlightRefs.Select(flightRef => Fare(person.TravellerRef, flightRef, bound.PortionRef)))).ToList(),
            TravellerBaggageFacts = people
                .SelectMany(person => bounds.SelectMany(bound => bound.FlightRefs.Select(flightRef => Baggage(person.TravellerRef, flightRef, bound.PortionRef))))
                .ToList(),
            CoverageCompleteness = new FactCompleteness
            {
                TravellersComplete = true,
                ItineraryComplete = true,
                FareFactsComplete = true,
                BaggageFactsComplete = true,
                FareBenefitsComplete = true,
                ExistingServicesComplete = true,
                Provenance = "TEST-FIXTURE"
            }
        };
    }

    public static TravellerFareFacts Fare(string travellerRef, string flightRef, string portionRef)
        => new()
        {
            TravellerRef = travellerRef,
            FlightRef = flightRef,
            PortionRef = portionRef,
            AirFareId = 501,
            AirFareType = (AirFareType)1,
            FareFamilyId = 5,
            FareFamilyCodeOrName = "FLEX",
            FareBasisCode = "YOW",
            CabinClassId = 1,
            RbdId = 11
        };

    public static TravellerFlightBaggageFacts Baggage(string travellerRef, string flightRef, string portionRef, int checkedPieces = 1, int cabinPieces = 1)
        => new()
        {
            TravellerRef = travellerRef,
            FlightRef = flightRef,
            PortionRef = portionRef,
            CheckedPieces = checkedPieces,
            CabinPieces = cabinPieces,
            SourceCompleteness = FactEvidence.Verified
        };

    public static CanonicalAncillaryOfferCandidate One(this CanonicalAncillaryOfferResult result, string? reference = null)
        => Xunit.Assert.Single(result.Candidates, candidate => reference is null || candidate.ServiceDefinitionRef == reference);
}
