using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.VerifySeatSelections
{
    public sealed class SeatSelectionVerifier : ISeatSelectionVerifier
    {
        private const string Provenance = "SeatMaps/Holds";
        private const string SeatMapAuthority = "AirInfo";

        private readonly ISeatMapPricingEngine _engine;
        private readonly IAircraftSeatMapProvider _seatMaps;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IAirportReference _airports;
        private readonly IAirFareReference _airFares;
        private readonly IClock _clock;

        public SeatSelectionVerifier(
            ISeatMapPricingEngine engine,
            IAircraftSeatMapProvider seatMaps,
            IOperatorAirlineResolver operatorAirline,
            IAirportReference airports,
            IAirFareReference airFares,
            IClock clock)
        {
            _engine = engine;
            _seatMaps = seatMaps;
            _operatorAirline = operatorAirline;
            _airports = airports;
            _airFares = airFares;
            _clock = clock;
        }

        public async Task<IReadOnlyList<VerifiedSeatSelection>> VerifyAsync(SeatSelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var selections = verification.Seats
                .Select(seat => new Selection(seat, SeatMapSelectionId.Decode(seat.SeatMapSelectionId) ?? throw NotSellable(seat.SeatMapSelectionId, "the id cannot be read")))
                .ToList();
            var flights = await FlightsAsync(selections, cancellationToken);
            var seats = await SeatsAsync(selections, flights, cancellationToken);
            var context = await ContextAsync(verification, (int)homeAirlineId.Value, selections, flights, seats, cancellationToken);
            var priced = await _engine.PriceSeatsAsync(context, OfferFamilyFilters.SeatMap, seats.Select(seat => seat.Seat).ToList(), cancellationToken);

            return selections.Select(selection => Verified(selection, flights[selection.Decoded.FlightId], priced.Seats)).ToList();
        }

        private static VerifiedSeatSelection Verified(Selection selection, ShoppingFlight flight, IReadOnlyList<SeatPricing> priced)
        {
            var decoded = selection.Decoded;
            var pricing = priced.FirstOrDefault(seat => seat.TravellerRef == decoded.TravellerRef
                                                        && seat.FlightRef == flight.FlightRef
                                                        && string.Equals(seat.SeatNumber, decoded.SeatNumber, StringComparison.OrdinalIgnoreCase));

            if (pricing is not { Status: SeatAvailabilityStatus.Available, Candidate: { } candidate })
                throw NotSellable(selection.Input.SeatMapSelectionId, pricing is { ReasonCodes.Count: > 0 } blocked ? string.Join(',', blocked.ReasonCodes) : "SeatSelectionNotOffered");

            if (candidate.Price.PricingRevisionId != decoded.PricingId)
                throw NotSellable(selection.Input.SeatMapSelectionId, "PriceChanged");

            return new VerifiedSeatSelection(
                selection.Input.OrderServiceId,
                selection.Input.TravellerId,
                selection.Input.SeatMapSelectionId,
                decoded.SeatNumber,
                decoded.FlightId,
                candidate.DefinitionVersionId,
                candidate.ProvisionId,
                decoded.PricingId);
        }

        private async Task<IReadOnlyDictionary<long, ShoppingFlight>> FlightsAsync(IReadOnlyList<Selection> selections, CancellationToken cancellationToken)
        {
            var airports = selections.SelectMany(selection => new[] { selection.Input.Flight.OriginAirportId, selection.Input.Flight.DestinationAirportId }).Distinct().ToList();
            var countries = await _airports.FindCountryIdsAsync(airports, cancellationToken);
            var zones = await _airports.FindTimeZoneIdsAsync(airports, cancellationToken);
            var flights = new Dictionary<long, ShoppingFlight>();

            foreach (var group in selections.GroupBy(selection => selection.Decoded.FlightId))
            {
                var facts = group.Select(selection => selection.Input.Flight).Distinct().ToList();

                if (facts.Count != 1)
                    throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(SeatHoldSelectionInput.Flight)}[{group.Key}]");

                var flight = facts[0];

                flights[group.Key] = new ShoppingFlight
                {
                    FlightRef = $"F{group.Key}",
                    FlightId = group.Key,
                    FlightNumber = flight.FlightNumber,
                    MarketingAirlineId = flight.MarketingAirlineId,
                    OperatingAirlineId = flight.OperatingAirlineId,
                    OriginAirportId = flight.OriginAirportId,
                    DestinationAirportId = flight.DestinationAirportId,
                    ViaAirportIds = [],
                    OriginCountryId = countries.TryGetValue(flight.OriginAirportId, out var origin) ? origin : null,
                    DestinationCountryId = countries.TryGetValue(flight.DestinationAirportId, out var destination) ? destination : null,
                    OriginIanaTimeZoneId = zones.GetValueOrDefault(flight.OriginAirportId),
                    DepartureAt = flight.DepartureAt,
                    ArrivalAt = flight.ArrivalAt,
                    AircraftId = flight.AircraftId,
                    CabinClassId = flight.CabinClassId,
                    RbdId = flight.RbdId
                };
            }

            return flights;
        }

        private async Task<IReadOnlyList<(SeatMapSeat Seat, IReadOnlyList<string> CharacteristicCodes)>> SeatsAsync(
            IReadOnlyList<Selection> selections,
            IReadOnlyDictionary<long, ShoppingFlight> flights,
            CancellationToken cancellationToken)
        {
            var maps = new Dictionary<int, AircraftSeatMap>();
            var seats = new List<(SeatMapSeat, IReadOnlyList<string>)>();

            foreach (var selection in selections.DistinctBy(selection => (selection.Decoded.FlightId, selection.Decoded.SeatNumber)))
            {
                var flight = flights[selection.Decoded.FlightId];
                var aircraftId = flight.AircraftId!.Value;

                if (!maps.TryGetValue(aircraftId, out var map))
                    maps[aircraftId] = map = await _seatMaps.GetAsync(aircraftId, cancellationToken);

                var seat = map.Decks
                    .SelectMany(deck => deck.Cabins.SelectMany(cabin => cabin.Items.Where(item => item.IsSeat).Select(item => (Cabin: cabin, Item: item))))
                    .FirstOrDefault(row => string.Equals(row.Item.Number!.Trim(), selection.Decoded.SeatNumber, StringComparison.OrdinalIgnoreCase));

                if (seat.Item is null)
                    throw NotSellable(selection.Input.SeatMapSelectionId, "the seat is not on the aircraft");

                seats.Add((new SeatMapSeat(flight.FlightRef, seat.Item.Number!.Trim(), seat.Cabin.CabinClassId, false), seat.Item.CharacteristicCodes));
            }

            return seats;
        }

        private async Task<AncillaryShoppingContext> ContextAsync(
            SeatSelectionVerification verification,
            int ownerAirlineId,
            IReadOnlyList<Selection> selections,
            IReadOnlyDictionary<long, ShoppingFlight> flights,
            IReadOnlyList<(SeatMapSeat Seat, IReadOnlyList<string> CharacteristicCodes)> seats,
            CancellationToken cancellationToken)
        {
            var travellers = new List<ShoppingTraveller>();

            foreach (var group in selections.GroupBy(selection => selection.Decoded.TravellerRef, StringComparer.Ordinal))
            {
                var facts = group.Select(selection => (selection.Input.TravellerId, selection.Input.PassengerTypeCode, selection.Input.DateOfBirth)).Distinct().ToList();

                if (facts.Count != 1)
                    throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(SeatHoldSelectionInput.TravellerId)}[{group.Key}]");

                travellers.Add(new ShoppingTraveller
                {
                    TravellerRef = group.Key,
                    OrderTravellerId = facts[0].TravellerId,
                    PassengerTypeCode = facts[0].PassengerTypeCode,
                    DateOfBirth = facts[0].DateOfBirth,
                    StableTravellerIdentity = $"{verification.OrderId}:{facts[0].TravellerId}"
                });
            }

            var fareFamilies = await _airFares.FindFareFamilyIdsAsync(
                selections.Select(selection => selection.Input.Fare?.AirFareId).OfType<long>().Distinct().ToList(),
                cancellationToken);
            var orderedFlights = flights.Values.OrderBy(flight => flight.DepartureAt).ToList();
            var portions = orderedFlights
                .Select((flight, index) => new ShoppingPortion
                {
                    PortionRef = $"P{flight.FlightId}",
                    Sequence = index + 1,
                    FlightRefs = [flight.FlightRef],
                    OriginAirportId = flight.OriginAirportId,
                    DestinationAirportId = flight.DestinationAirportId,
                    ViaAirportIds = []
                })
                .ToList();
            var fares = selections
                .DistinctBy(selection => (selection.Decoded.TravellerRef, selection.Decoded.FlightId))
                .Select(selection => new TravellerFareFacts
                {
                    TravellerRef = selection.Decoded.TravellerRef,
                    FlightRef = flights[selection.Decoded.FlightId].FlightRef,
                    PortionRef = $"P{selection.Decoded.FlightId}",
                    AirFareId = selection.Input.Fare?.AirFareId,
                    AirFareType = selection.Input.Fare?.AirFareType,
                    FareFamilyId = selection.Input.Fare is { } fare && fareFamilies.TryGetValue(fare.AirFareId, out var fareFamilyId) ? fareFamilyId : null,
                    FareBasisCode = selection.Input.Fare?.FareBasis,
                    CabinClassId = selection.Input.Flight.CabinClassId,
                    RbdId = selection.Input.Flight.RbdId,
                    BookingClass = selection.Input.Fare?.BookingClass,
                    CouponOpenForUse = selection.Input.CouponOpenForUse
                })
                .ToList();

            return new AncillaryShoppingContext
            {
                OwnerAirlineId = ownerAirlineId,
                PointOfSaleId = verification.PointOfSaleId,
                ShoppingStage = verification.TicketedAtUtc is null ? ShoppingStage.PreOrder : ShoppingStage.PostTicketed,
                EvaluatedAtUtc = _clock.GetDateTime().ToUniversalTime(),
                CurrencyId = verification.CurrencyId,
                SourceIdentity = new SourceIdentityContext
                {
                    SourceKind = ShoppingSourceKind.Order,
                    OrderId = verification.OrderId,
                    OrderCommercialVersion = verification.OrderCommercialVersion,
                    TrustedSourceReference = $"order:{verification.OrderId}",
                    TicketedAtUtc = verification.TicketedAtUtc?.ToUniversalTime()
                },
                Travellers = travellers,
                Portions = portions,
                Flights = orderedFlights,
                TravellerFareFacts = fares,
                SeatCharacteristicFacts = seats
                    .Select(seat => new SeatCharacteristicFacts
                    {
                        FlightRef = seat.Seat.FlightRef,
                        SeatNumber = seat.Seat.SeatNumber,
                        CharacteristicCodes = seat.CharacteristicCodes,
                        SourceCompleteness = FactEvidence.Verified,
                        SourceAuthority = SeatMapAuthority
                    })
                    .ToList(),
                CoverageCompleteness = new FactCompleteness
                {
                    TravellersComplete = true,
                    ItineraryComplete = true,
                    FareFactsComplete = selections.All(selection => selection.Input.Fare is not null),
                    BaggageFactsComplete = false,
                    FareBenefitsComplete = false,
                    ExistingServicesComplete = false,
                    Provenance = Provenance
                }
            };
        }

        private static Exception NotSellable(string seatMapSelectionId, string reason)
            => ExceptionFactory.AncillaryHoldSeatNotSellable(seatMapSelectionId, reason);

        private sealed record Selection(SeatHoldSelectionInput Input, SeatMapSelection Decoded);
    }
}
