using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
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
using AeroTech.Messages.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;
using SeatMapControllers = AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers;

namespace AeroTech.Ancillary.Shopping.Tests;

public class SeatMapTests
{
    private static readonly ShoppingFilter SeatMapFamily = new() { VariantCodes = ["A07", "A08"] };

    [Fact]
    public async Task SM_01_a_seat_takes_its_most_specific_category_and_is_blocked_when_that_category_excludes_the_traveller_or_no_category_prices_it()
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
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T1", "14D"));
        Assert.Equal((SeatAvailabilityStatus.Occupied, null, null), Of("T1", "15E"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T1", "1A"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T2", "12A"));
        Assert.Equal((SeatAvailabilityStatus.Available, "A07", 10m), Of("T2", "12B"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T2", "12C"));
        Assert.Equal((SeatAvailabilityStatus.Blocked, null, null), Of("T2", "14D"));
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
            new[] { (SeatAvailabilityStatus.Available, (string?)standard.ServiceDefinitionRef, (decimal?)4m), (SeatAvailabilityStatus.Blocked, null, null) },
            result.Seats.Select(seat => (seat.Status, seat.Candidate?.ServiceDefinitionRef, seat.Candidate?.Price.CompleteUnitTotal)));
    }

    [Fact]
    public async Task SM_02_without_any_seat_product_no_seat_is_selectable()
    {
        var lab = new ShoppingLab();
        var context = Trip.Context() with { SeatCharacteristicFacts = [Trip.Seat("12A", codes: ["W"])] };

        var result = await lab.Engine.PriceSeatsAsync(context, SeatMapFamily, [new SeatMapSeat("F1", "12A", 1, false), new SeatMapSeat("F1", "1A", 3, false)]);

        Assert.Equal(
            new[] { (SeatAvailabilityStatus.Blocked, (string?)null), (SeatAvailabilityStatus.Blocked, null) },
            result.Seats.Select(seat => (seat.Status, seat.Candidate?.VariantCode)));
        Assert.Empty(result.RatesOfExchange);
    }

    [Fact]
    public async Task SM_03_the_seat_map_is_the_aircraft_layout_with_a_price_and_status_for_every_traveller_and_a_recoverable_id_on_selectable_seats_only()
    {
        var lab = new ShoppingLab();
        var standard = lab.Definition("A07");
        var middle = lab.Definition("A07", "SEAT_STANDARD_MIDDLE");

        var paid = lab.Price(standard, lab.Provision(standard), Rate(15m));

        lab.Price(middle, lab.Provision(middle, 20, Rules(seats: new ProvisionSeatApplicationArgs([], ["9"]))), Rate(0m));

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
        Assert.Equal(("AVAILABLE", "0.00", "0.00"), (Seat("3B", "ADT-1").SeatAvailabilityStatus, Seat("3B", "ADT-1").Price!.Base, Seat("3B", "ADT-1").Price!.Total));
        Assert.Equal(("OCCUPIED", (string?)null, (SeatMapPriceDto?)null), (Seat("3C", "ADT-1").SeatAvailabilityStatus, Seat("3C", "ADT-1").Id, Seat("3C", "ADT-1").Price));
        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (Seat("1A", "CHD-1").SeatAvailabilityStatus, Seat("1A", "CHD-1").Id, Seat("1A", "CHD-1").Price));
        Assert.Empty(economy.Items.Single(item => item.Code == "Galley").TravelerPricing);
        Assert.Equal(new[] { "ADT-1", "CHD-1" }, Seat3ATravellers(first));
        Assert.Equal(new[] { ("ADT-1", 2), ("CHD-1", 2) }, first.AvailableSeatsCounters.Select(counter => (counter.TravelerId, counter.Value)));
        Assert.Equal(new[] { "E", "W" }, map.Dictionaries.SeatCharacteristics.Keys);

        var ids = map.Data.SelectMany(data => data.Decks).SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).SelectMany(item => item.TravelerPricing).Select(row => row.Id).OfType<string>().ToList();

        Assert.Equal(8, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ids, id => Assert.Matches("^[A-Za-z0-9_-]+$", id));
        Assert.Equal(
            ("ADT-1", snapshot.Flights[0].Flight.FlightId, "3A", paid.Id),
            SeatMapSelectionId.Decode(Seat("3A", "ADT-1").Id) is { } decoded ? (decoded.TravellerRef, decoded.FlightId, decoded.SeatNumber, decoded.PricingId) : default);
        Assert.Equal("3B", SeatMapSelectionId.Decode(Seat("3B", "CHD-1").Id)?.SeatNumber);

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
    public async Task SM_09_an_occupancy_answer_that_misses_a_requested_flight_fails_the_seat_map_instead_of_showing_its_seats_free()
    {
        var (service, _) = await SeatMapServiceAsync(new ShoppingLab(), new PartialOccupiedSeats());

        var failure = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale));

        Assert.Equal((16814, 502), (failure.Code, failure.HttpStatus));
    }

    [Fact]
    public async Task SM_10_a_seat_is_blocked_when_the_booked_cabin_of_its_traveller_is_unknown()
    {
        var lab = new ShoppingLab();
        var context = Trip.Context([Trip.Flight() with { CabinClassId = null }]);

        context = context with
        {
            TravellerFareFacts = context.TravellerFareFacts.Select(fact => fact with { CabinClassId = null }).ToList(),
            SeatCharacteristicFacts = [Trip.Seat("12A", codes: ["W"])]
        };

        var result = await lab.Engine.PriceSeatsAsync(context, SeatMapFamily, [new SeatMapSeat("F1", "12A", 1, false)]);

        Assert.Equal((SeatAvailabilityStatus.Blocked, (string?)null), (result.Seats.Single().Status, result.Seats.Single().Candidate?.VariantCode));
    }

    [Fact]
    public async Task SM_11_a_seat_whose_category_has_no_rate_is_blocked_and_so_is_a_seat_outside_every_category()
    {
        var lab = new ShoppingLab();
        var standard = lab.Definition("A07");

        lab.Provision(standard, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["W"])));

        var (service, _) = await SeatMapServiceAsync(lab, new FixedOccupiedSeats());
        var map = SeatMapMapper.Map(await service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale), travellerRef => travellerRef, flight => flight.FlightRef);
        var windowSeat = Pricing(map.Data[0], "3A", "ADT-1");
        var plainSeat = Pricing(map.Data[0], "3B", "ADT-1");

        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (windowSeat.SeatAvailabilityStatus, windowSeat.Id, windowSeat.Price));
        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (plainSeat.SeatAvailabilityStatus, plainSeat.Id, plainSeat.Price));
    }

    [Fact]
    public async Task SM_12_a_seat_two_points_of_sale_price_differently_is_blocked_and_a_seat_they_price_alike_keeps_its_price_while_an_uncategorized_seat_is_blocked()
    {
        var lab = new ShoppingLab();
        var window = lab.Definition("A07");
        var middle = lab.Definition("A07", "SEAT_STANDARD_MIDDLE");

        foreach (var (pointOfSale, sequence, amount) in new[] { (Pos, 10, 15m), (OtherPos, 20, 20m) })
        {
            lab.Price(window, lab.Provision(window, sequence, Rules(seats: new ProvisionSeatApplicationArgs([], ["W"])), pos: pointOfSale), Rate(amount));
            lab.Price(middle, lab.Provision(middle, sequence, Rules(seats: new ProvisionSeatApplicationArgs([], ["9"])), pos: pointOfSale), Rate(30m));
        }

        var (service, _) = await SeatMapServiceAsync(lab, new FixedOccupiedSeats());
        var snapshot = await service.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale.On(Channel.BackOffice, Pos, OtherPos));
        var map = SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.FlightRef);
        var ambiguous = Pricing(map.Data[0], "3A", "ADT-1");
        var agreed = Pricing(map.Data[0], "3B", "ADT-1");
        var uncategorized = Pricing(map.Data[0], "3C", "ADT-1");

        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (ambiguous.SeatAvailabilityStatus, ambiguous.Id, ambiguous.Price));
        Assert.Equal(("AVAILABLE", "30.00"), (agreed.SeatAvailabilityStatus, agreed.Price?.Total));
        Assert.Equal(("BLOCKED", (string?)null, (SeatMapPriceDto?)null), (uncategorized.SeatAvailabilityStatus, uncategorized.Id, uncategorized.Price));
    }

    private static SeatMapTravelerPricingDto Pricing(SeatMapDto map, string seatNumber, string travellerId)
        => map.Decks.SelectMany(deck => deck.Cabins).SelectMany(cabin => cabin.Items).Single(item => item.Number == seatNumber).TravelerPricing.Single(row => row.TravelerId == travellerId);

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
