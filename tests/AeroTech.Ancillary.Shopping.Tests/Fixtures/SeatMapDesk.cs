using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Messages.AirPrice.Enums;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class SeatMapDesk
{
    public sealed class FixedSeatMaps(AircraftSeatMap map) : IAircraftSeatMapProvider
    {
        public List<int> Asked { get; } = [];

        public Task<AircraftSeatMap> GetAsync(int aircraftId, CancellationToken cancellationToken = default)
        {
            Asked.Add(aircraftId);

            return Task.FromResult(map);
        }
    }

    public sealed class FixedOccupiedSeats(params string[] seats) : IOccupiedSeatProvider
    {
        public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<long, IReadOnlySet<string>>>(
                flightIds.ToDictionary(flightId => flightId, _ => (IReadOnlySet<string>)seats.ToHashSet(StringComparer.OrdinalIgnoreCase)));
    }

    public sealed class FailingOccupiedSeats : IOccupiedSeatProvider
    {
        public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
            => throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider is unreachable");
    }

    public sealed class PartialOccupiedSeats : IOccupiedSeatProvider
    {
        public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<long, IReadOnlySet<string>>>(
                flightIds.Take(1).ToDictionary(flightId => flightId, _ => (IReadOnlySet<string>)new HashSet<string>()));
    }

    public static AircraftCabinItem SeatItem(string number, int row, params string[] codes)
        => new(number, AircraftCabinItem.SeatCode, 0, row, null, codes, [], 0, row, 1);

    public static AircraftSeatMap Aircraft()
        => new(
            [
                new AircraftDeck(
                    "1",
                    new AircraftDeckConfiguration(6, 30, 1, 30, 0, 0, 10, 14, [12]),
                    [
                        new AircraftCabin("Business", 3, 1, 2, [SeatItem("1A", 1, "E")]),
                        new AircraftCabin(
                            "Economy",
                            1,
                            3,
                            30,
                            [
                                SeatItem("3A", 3, "E", "W"),
                                SeatItem("3B", 3, "9"),
                                SeatItem("3C", 3, "E", "A"),
                                new AircraftCabinItem(null, "Galley", 0, 4, null, [], [], 0, 4, 1)
                            ])
                    ])
            ],
            new Dictionary<string, string> { ["G"] = "Galley" },
            new Dictionary<string, string> { ["E"] = "Exit row seat", ["W"] = "Window seat" });

    public sealed class VerifiedExitRows(IShoppingSource source) : IShoppingSource
    {
        public async Task<FlightOfferShopping> LoadAsync(string? offerId, long? orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var shopping = await source.LoadAsync(offerId, orderId, scope, cancellationToken);

            return shopping with
            {
                PointsOfSale = shopping.PointsOfSale
                    .Select(pointOfSale => pointOfSale with
                    {
                        Context = pointOfSale.Context with
                        {
                            Travellers = pointOfSale.Context.Travellers.Select(traveller => traveller with { VerifiedExitRowEligible = traveller.PassengerTypeCode == PassengerTypeCode.ADT }).ToList()
                        }
                    })
                    .ToList()
            };
        }
    }

    public static async Task<(IShoppingSource Source, TestOnlyAirportReference Airports)> SourceAsync(
        ShoppingLab lab,
        FlightOffer? offer = null,
        IOrderCommercialContextReader? orders = null,
        bool verifiedExitRows = false)
    {
        var airports = new TestOnlyAirportReference();
        var flightOffers = new FlightOfferShoppingSource(
            new FixedOffers(offer ?? Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()))),
            airports,
            new TestOnlyAirFareReference(),
            new FixedClock { Now = Sale.EvaluatedAtUtc },
            new FlightOfferShoppingOptions(SourceContextTtl));

        airports.IataCodes[2] = "IKA";
        airports.IataCodes[6] = "IST";
        airports.TerminalNumbers[2] = "1";
        airports.TerminalNumbers[6] = "2";
        lab.Catalog.AirlineCodes[1] = "DA";
        lab.Catalog.AirlineCodes[3] = "IR";

        return (verifiedExitRows ? new VerifiedExitRows(Sources(flightOffers, orders)) : Sources(flightOffers, orders), airports);
    }

    public static async Task<(GetSeatMapService Service, FixedSeatMaps Maps)> SeatMapServiceAsync(ShoppingLab lab, IOccupiedSeatProvider? occupied = null)
    {
        var (source, airports) = await SourceAsync(lab);
        var maps = new FixedSeatMaps(Aircraft());

        return (new GetSeatMapService(source, maps, occupied ?? new FixedOccupiedSeats("3C"), lab.Engine, lab.Catalog, airports, lab.Catalog), maps);
    }
}
