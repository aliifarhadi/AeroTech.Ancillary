using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2ExitRowSeatTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static async Task<(ShoppingLab Lab, SeatTravellerOffer Seat)> SeatAsync(string traveller, bool verifiedExitRows = true)
    {
        var lab = new ShoppingLab();
        var exitRow = lab.Definition("A08");

        lab.Price(exitRow, lab.Provision(exitRow, stage: PurchaseStage.Both), Rate(40m));

        var orders = new FixedOrders(OrderOf(Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()))));
        var (_, seatMaps) = await SeatResolveAsync(lab, orders: orders, verifiedExitRows: verifiedExitRows);
        var map = await seatMaps.ExecuteAsync(new BackofficeGetSeatMapQuery { OrderId = OrderDesk.OrderId }, Sale);

        return (lab, map.Flights[0].OffersOf("3A").Single(offer => offer.TravellerRef == traveller));
    }

    [Fact]
    public async Task B2_a_verified_adult_gets_an_exit_row_seat_whose_resolution_states_the_exit_row_requirement_and_its_price()
    {
        var (lab, seat) = await SeatAsync(Adult);
        var (service, _) = await SeatResolveAsync(lab);

        var resolved = ServiceSeatMapResolutionMapper.Map(await service.ExecuteAsync([seat.Id!])).Seats.Single();

        Assert.Equal(SeatAvailabilityStatus.Available, seat.Status);
        Assert.Equal(("SEAT_PREFERRED", true, 40m), (resolved.ServiceDefinition.ServiceDefinitionRef, resolved.ServiceDefinition.Seat!.RequiresExitRowEligibility, resolved.Pricing.Rates.Single().UnitTotal));
    }

    [Fact]
    public async Task B2_an_unverified_adult_and_an_ineligible_child_never_see_an_exit_row_seat_as_selectable()
    {
        var (_, unverifiedAdult) = await SeatAsync(Adult, verifiedExitRows: false);
        var (_, child) = await SeatAsync(Child);

        Assert.Equal((SeatAvailabilityStatus.Blocked, (string?)null), (unverifiedAdult.Status, unverifiedAdult.Id));
        Assert.Equal((SeatAvailabilityStatus.Blocked, (string?)null), (child.Status, child.Id));
    }
}
