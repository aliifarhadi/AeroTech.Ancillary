using System.Globalization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;
using SeatMapControllers = AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers;

namespace AeroTech.Ancillary.Shopping.Tests;

public class SeatMapTests
{
    private static readonly ShoppingFilter SeatMapFamily = new() { VariantCodes = ["A07", "A08"] };

    private sealed class FixedSeatMaps(AircraftSeatMap map) : IAircraftSeatMapProvider
    {
        public List<int> Asked { get; } = [];

        public Task<AircraftSeatMap> GetAsync(int aircraftId, CancellationToken cancellationToken = default)
        {
            Asked.Add(aircraftId);

            return Task.FromResult(map);
        }
    }

    private sealed class FixedOccupiedSeats(params string[] seats) : IOccupiedSeatProvider
    {
        public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<long, IReadOnlySet<string>>>(
                flightIds.ToDictionary(flightId => flightId, _ => (IReadOnlySet<string>)seats.ToHashSet(StringComparer.OrdinalIgnoreCase)));
    }

    private sealed class FailingOccupiedSeats : IOccupiedSeatProvider
    {
        public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
            => throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider is unreachable");
    }

    private static AircraftCabinItem SeatItem(string number, int row, params string[] codes)
        => new(number, AircraftCabinItem.SeatCode, 0, row, null, codes, [], 0, row, 1);

    private static AircraftSeatMap Aircraft()
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

    private static async Task<(GetSeatMapService Service, FixedSeatMaps Maps)> SeatMapServiceAsync(ShoppingLab lab, IOccupiedSeatProvider? occupied = null)
    {
        var maps = new FixedSeatMaps(Aircraft());
        var airports = new TestOnlyAirportReference();
        var source = new FlightOfferShoppingSource(
            new FixedOffers(Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()))),
            airports,
            new TestOnlyAirFareReference());

        airports.IataCodes[2] = "IKA";
        airports.IataCodes[6] = "IST";
        airports.TerminalNumbers[2] = "1";
        airports.TerminalNumbers[6] = "2";
        lab.Catalog.AirlineCodes[1] = "DA";
        lab.Catalog.AirlineCodes[3] = "IR";

        return (new GetSeatMapService(source, maps, occupied ?? new FixedOccupiedSeats("3C"), lab.Engine, lab.Catalog, airports, lab.Catalog), maps);
    }

    [Fact]
    public async Task SM_01_a_seat_takes_its_most_specific_category_and_is_blocked_when_that_category_excludes_the_traveller()
    {
        var lab = new ShoppingLab();
        var standard = lab.Definition("A07");
        var preferred = lab.Definition("A08");

        lab.Price(standard, lab.Provision(standard, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["9", "A"]))), Rate(10m));
        lab.Price(preferred, lab.Provision(preferred, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["L"]))), Rate(30m));

        var context = Trip.Context(travellers: [Trip.Adult(), Trip.Child()]) with
        {
            SeatCharacteristicFacts =
            [
                Trip.Seat("12A", codes: ["L", "W"]), Trip.Seat("12B", codes: ["9"]), Trip.Seat("12C", codes: ["A", "L"]),
                Trip.Seat("14D", codes: ["W"]), Trip.Seat("15E", codes: ["9"]), Trip.Seat("1A", codes: ["9"])
            ]
        };
        var seats = new SeatMapSeat[]
        {
            new("F1", "12A", 1, false), new("F1", "12B", 1, false), new("F1", "12C", 1, false),
            new("F1", "14D", 1, false), new("F1", "15E", 1, true), new("F1", "1A", 4, false)
        };

        var result = await lab.Engine.PriceSeatsAsync(context, SeatMapFamily, seats);

        (SeatAvailabilityStatus, string?, decimal?) Of(string traveller, string seat)
        {
            var priced = result.Seats.Single(row => row.TravellerRef == traveller && row.SeatNumber == seat);

            return (priced.Status, priced.Candidate?.VariantCode, priced.Candidate?.Price.CompleteUnitTotal);
        }

        Assert.Equal(12, result.Seats.Count);
        Assert.Equal((SeatAvailabilityStatus.Available, "A08", 30m), Of("T1", "12A"));
        Assert.Equal((SeatAvailabilityStatus.Available, "A07", 10m), Of("T1", "12B"));
        Assert.Equal((SeatAvailabilityStatus.Available, "A08", 30m), Of("T1", "12C"));
        Assert.Equal((SeatAvailabilityStatus.Available, null, null), Of("T1", "14D"));
        Assert.Equal((SeatAvailabilityStatus.Occupied, null, null), Of("T1", "15E"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T1", "1A"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T2", "12A"));
        Assert.Equal((SeatAvailabilityStatus.Available, "A07", 10m), Of("T2", "12B"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T2", "12C"));
        Assert.Equal((SeatAvailabilityStatus.Available, null, null), Of("T2", "14D"));
    }

    [Fact]
    public async Task SM_07_each_fare_family_gets_its_own_provision_and_a_category_of_another_cabin_never_blocks_a_seat()
    {
        var lab = new ShoppingLab();
        var standard = lab.Definition("A07");
        var premium = lab.Definition("A07", "SEAT_PREMIUM_CABIN");
        ProvisionRulesArgs ForFamily(long fareFamilyId)
            => Rules(fares: new ProvisionFareApplicationArgs([], [], [fareFamilyId], [], [], []), seats: new ProvisionSeatApplicationArgs([], ["W", "9"]));

        lab.Price(standard, lab.Provision(standard, 10, ForFamily(1)), Rate(9m));
        lab.Price(standard, lab.Provision(standard, 20, ForFamily(5)), Rate(4m));
        lab.Provision(premium, 10, Rules(fares: new ProvisionFareApplicationArgs([], [], [], [], [3], []), seats: new ProvisionSeatApplicationArgs([], ["W"])), disposition: CommercialDisposition.Free);

        var exitRow = lab.Definition("A08", "SEAT_EXIT_ROW_OTHER_CABIN", specification => specification with { Seat = specification.Seat! with { ApplicableCabinIds = [3] } });

        lab.Provision(exitRow, 10, Rules(seats: new ProvisionSeatApplicationArgs([], ["W", "E"])), disposition: CommercialDisposition.Free);

        var context = Trip.Context() with
        {
            TravellerFareFacts = [Trip.Fare("T1", "F1", "P1")],
            SeatCharacteristicFacts = [Trip.Seat("12A", codes: ["W"]), Trip.Seat("14D", codes: ["A"])]
        };

        var result = await lab.Engine.PriceSeatsAsync(context, SeatMapFamily, [new SeatMapSeat("F1", "12A", 1, false), new SeatMapSeat("F1", "14D", 1, false)]);

        Assert.Equal(
            new[] { (SeatAvailabilityStatus.Available, (string?)standard.ServiceDefinitionRef, (decimal?)4m), (SeatAvailabilityStatus.Available, null, null) },
            result.Seats.Select(seat => (seat.Status, seat.Candidate?.ServiceDefinitionRef, seat.Candidate?.Price.CompleteUnitTotal)));
    }

    [Fact]
    public async Task SM_02_without_any_seat_product_every_free_seat_of_the_booked_cabin_is_available_at_no_price()
    {
        var lab = new ShoppingLab();
        var context = Trip.Context() with { SeatCharacteristicFacts = [Trip.Seat("12A", codes: ["W"])] };

        var result = await lab.Engine.PriceSeatsAsync(context, SeatMapFamily, [new SeatMapSeat("F1", "12A", 1, false), new SeatMapSeat("F1", "1A", 3, false)]);

        Assert.Equal(
            new[] { (SeatAvailabilityStatus.Available, (string?)null), (SeatAvailabilityStatus.Blocked, null) },
            result.Seats.Select(seat => (seat.Status, seat.Candidate?.VariantCode)));
        Assert.Empty(result.RatesOfExchange);
    }

    [Fact]
    public async Task SM_03_the_seat_map_is_the_aircraft_layout_with_a_price_and_status_for_every_traveller_and_a_hold_id_on_free_seats_only()
    {
        var lab = new ShoppingLab();
        var standard = lab.Definition("A07");

        lab.Price(standard, lab.Provision(standard), Rate(15m));

        var (service, maps) = await SeatMapServiceAsync(lab);
        var snapshot = await service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale);
        var map = SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.FlightRef);
        var first = map.Data[0];
        var economy = first.Decks.Single().Cabins.Single(cabin => cabin.CabinClassId == 1);

        SeatMapTravelerPricingDto Seat(string number, string traveller)
            => first.Decks.SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).Single(item => item.Number == number).TravelerPricing.Single(row => row.TravelerId == traveller);

        Assert.Equal(new[] { 1 }, maps.Asked);
        Assert.Equal((2, 2, SeatMapMapper.SeatMapType, OfferWire.OfferId), (map.Meta.Count, map.Data.Count, first.Type, first.FlightOfferId));
        Assert.Equal(("1", 12, 30), (first.Decks[0].DeckType, first.Decks[0].DeckConfiguration.ExitRowsX.Single(), first.Decks[0].DeckConfiguration.Length));
        Assert.Equal(("AVAILABLE", "EUR", "15.00", "15.00"), (Seat("3A", "ADT-1").SeatAvailabilityStatus, Seat("3A", "ADT-1").Price!.Currency, Seat("3A", "ADT-1").Price!.Base, Seat("3A", "ADT-1").Price!.Total));
        Assert.Equal(("AVAILABLE", "0", "0"), (Seat("3B", "ADT-1").SeatAvailabilityStatus, Seat("3B", "ADT-1").Price!.Base, Seat("3B", "ADT-1").Price!.Total));
        Assert.Equal(("OCCUPIED", (string?)null, (SeatMapPriceDto?)null), (Seat("3C", "ADT-1").SeatAvailabilityStatus, Seat("3C", "ADT-1").Id, Seat("3C", "ADT-1").Price));
        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (Seat("1A", "CHD-1").SeatAvailabilityStatus, Seat("1A", "CHD-1").Id, Seat("1A", "CHD-1").Price));
        Assert.Empty(economy.Items.Single(item => item.Code == "Galley").TravelerPricing);
        Assert.Equal(new[] { "ADT-1", "CHD-1" }, Seat3ATravellers(first));
        Assert.Equal(new[] { ("ADT-1", 2), ("CHD-1", 2) }, first.AvailableSeatsCounters.Select(counter => (counter.TravelerId, counter.Value)));
        Assert.Equal(new[] { "E", "W" }, map.Dictionaries.SeatCharacteristics.Keys);

        var ids = map.Data.SelectMany(data => data.Decks).SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).SelectMany(item => item.TravelerPricing).Select(row => row.Id).OfType<string>().ToList();

        Assert.Equal(8, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ids, id => Assert.Matches("^[A-Za-z0-9_-]{43}$", id));

        var again = SeatMapMapper.Map(await service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale), travellerRef => travellerRef, flight => flight.FlightRef);

        Assert.Equal(Seat("3A", "ADT-1").Id, again.Data[0].Decks.SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).Single(item => item.Number == "3A").TravelerPricing.Single(row => row.TravelerId == "ADT-1").Id);
        Assert.NotEqual(Seat("3A", "ADT-1").Id, Seat("3A", "CHD-1").Id);
        Assert.NotEqual(Seat("3A", "ADT-1").Id, Seat("3B", "ADT-1").Id);
    }

    private static IEnumerable<string> Seat3ATravellers(SeatMapDto map)
        => map.Decks.SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).Single(item => item.Number == "3A").TravelerPricing.Select(row => row.TravelerId);

    [Fact]
    public async Task SM_04_the_agency_surface_names_travellers_and_segments_the_way_its_offers_answer_does()
    {
        var lab = new ShoppingLab();
        var (service, _) = await SeatMapServiceAsync(lab);
        var snapshot = await service.ExecuteAsync(new ApiGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale);
        var travelerIds = ApiOfferIdentifiers.TravelerIds(snapshot.Offer);
        var segmentIds = ApiOfferIdentifiers.Segments(snapshot.Offer).GroupBy(segment => segment.FlightRef).ToDictionary(group => group.Key, group => group.First().Id);
        var map = SeatMapMapper.Map(snapshot, travellerRef => travelerIds[travellerRef], flight => segmentIds[flight.FlightRef]);

        Assert.Equal(new[] { "1", "2" }, map.Data.Select(data => data.SegmentId));
        Assert.All(map.Data, data => Assert.Equal(new[] { "1", "2" }, data.AvailableSeatsCounters.Select(counter => counter.TravelerId)));
    }

    [Fact]
    public async Task SM_08_each_seat_map_names_its_flight_with_its_endpoints_carriers_and_booking_class()
    {
        var lab = new ShoppingLab();
        var (service, _) = await SeatMapServiceAsync(lab);
        var snapshot = await service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale);
        var map = SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.Flight.FlightId.ToString(CultureInfo.InvariantCulture));

        Assert.Equal(
            new[]
            {
                ("1558420526869774453", "IKA", "1", "IST", "2", "DA", "1124", "DA", "Y", (int?)1),
                ("1550116065952923910", "IST", "2", "IKA", "1", "IR", "1598", "DA", "Y", (int?)1)
            },
            map.Data.Select(data => (data.SegmentId, data.Departure.IataCode, data.Departure.Terminal, data.Arrival.IataCode, data.Arrival.Terminal, data.CarrierCode, data.Number, data.Operating.CarrierCode, data.Class, data.CabinClassId)));
        Assert.Equal(
            new[] { (new DateTimeOffset(2026, 10, 15, 8, 15, 0, TimeSpan.FromHours(3.5)), new DateTimeOffset(2026, 10, 15, 11, 11, 0, TimeSpan.FromHours(3))) },
            map.Data.Take(1).Select(data => (data.Departure.At, data.Arrival.At)));
        Assert.All(map.Data, data => Assert.Equal(OfferWire.OfferId, data.FlightOfferId));
    }

    [Fact]
    public async Task SM_05_a_missing_occupancy_answer_fails_the_seat_map_instead_of_showing_every_seat_free()
    {
        var (service, _) = await SeatMapServiceAsync(new ShoppingLab(), new FailingOccupiedSeats());

        var failure = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OtaPanelGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale));

        Assert.Equal((16814, 502), (failure.Code, failure.HttpStatus));
    }

    [Fact]
    public void SM_06_three_seat_map_controllers_expose_one_operation_on_three_routes_under_three_policies()
    {
        var controllers = new[] { typeof(SeatMapControllers.BackofficeController), typeof(SeatMapControllers.OtaController), typeof(SeatMapControllers.OtaPanelController) }
            .Select(type => (
                Route: type.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template,
                Policy: type.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy,
                Actions: type.GetMethods().SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false)).Count()))
            .ToList();

        Assert.Equal(
            new[]
            {
                ("Backoffice/v{version:apiVersion}/SeatMaps", SurfaceAuthorization.Backoffice, 1),
                ("Api/v{version:apiVersion}/SeatMaps", SurfaceAuthorization.Ota, 1),
                ("OtaPanel/v{version:apiVersion}/SeatMaps", SurfaceAuthorization.OtaPanel, 1)
            },
            controllers.Select(controller => (controller.Route, controller.Policy!, controller.Actions)));
    }
}
