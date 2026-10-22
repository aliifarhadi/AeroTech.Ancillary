using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class GroupedDesk
{
    private GroupedDesk(
        IShoppingSource source,
        GetGroupedAncillaryOffersService grouped,
        GetGroupedOfferDetailService details,
        ResolveGroupedSelectionsService resolve,
        IGroupedSelectionVerifier verifier)
    {
        Source = source;
        Grouped = grouped;
        Details = details;
        Resolve = resolve;
        Verifier = verifier;
    }

    public IShoppingSource Source { get; }

    public GetGroupedAncillaryOffersService Grouped { get; }

    public GetGroupedOfferDetailService Details { get; }

    public ResolveGroupedSelectionsService Resolve { get; }

    public IGroupedSelectionVerifier Verifier { get; }

    public static async Task<GroupedDesk> CreateAsync(ShoppingLab lab, FlightOffer? offer = null, IOrderCommercialContextReader? orders = null)
    {
        var (source, _) = await SourceAsync(lab, offer, orders);
        var offers = Offers(source, lab);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var selections = new EvaluateSelectionService(details, lab.Engine);
        var selectionIds = new GroupedSelectionIdProvider(selections);
        var batches = new EvaluateBatchService(source, offers, details, selections, Tokens(new Application.AcceptanceTests.Fakes.FixedClock { Now = Sale.EvaluatedAtUtc }));
        var home = new HomeAirline();

        return new GroupedDesk(
            source,
            new GetGroupedAncillaryOffersService(offers, lab.Catalog, selectionIds),
            new GetGroupedOfferDetailService(details, selections, selectionIds),
            new ResolveGroupedSelectionsService(new ResolveSeatMapSelectionsService(lab.Catalog, lab.Catalog, lab.Catalog), new GroupedSelectionRevalidation(batches, home)),
            new GroupedSelectionVerifier(orders ?? new OrderDesk.FixedOrders(), home, source, lab.Engine, lab.Catalog));
    }
}
