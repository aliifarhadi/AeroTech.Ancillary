using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class SelectionDesk
{
    public const string SigningKey = "ancillary-priced-selection-test-signing-key";

    public static readonly OfferQuery ByOrder = new(null, OrderId: OrderDesk.OrderId);

    public static PricedSelectionTokens Tokens(FixedClock clock, string? signingKey = SigningKey) => new(new PricedSelectionOptions(signingKey), clock);

    public static async Task<(EvaluateBatchService Service, GetSeatMapService SeatMaps)> BatchAsync(
        ShoppingLab lab,
        IOccupiedSeatProvider? occupied = null,
        FlightOffer? offer = null,
        FixedClock? clock = null,
        IOrderCommercialContextReader? orders = null,
        string? signingKey = SigningKey,
        bool verifiedExitRows = false)
    {
        var (source, airports) = await SourceAsync(lab, offer, orders, verifiedExitRows);
        var offers = Offers(source, lab);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var seatMaps = new GetSeatMapService(source, new FixedSeatMaps(Aircraft()), occupied ?? new FixedOccupiedSeats(), lab.Engine, lab.Catalog, airports, lab.Catalog);
        var service = new EvaluateBatchService(
            source,
            offers,
            details,
            new EvaluateSelectionService(details, lab.Engine),
            Tokens(clock ?? new FixedClock { Now = Sale.EvaluatedAtUtc }, signingKey));

        return (service, seatMaps);
    }

    public static async Task<(ResolveSeatMapSelectionsService Service, GetSeatMapService SeatMaps)> SeatResolveAsync(
        ShoppingLab lab,
        IOccupiedSeatProvider? occupied = null,
        FlightOffer? offer = null,
        IOrderCommercialContextReader? orders = null,
        bool verifiedExitRows = false)
    {
        var (source, airports) = await SourceAsync(lab, offer, orders, verifiedExitRows);
        var seatMaps = new GetSeatMapService(source, new FixedSeatMaps(Aircraft()), occupied ?? new FixedOccupiedSeats(), lab.Engine, lab.Catalog, airports, lab.Catalog);

        return (new ResolveSeatMapSelectionsService(lab.Catalog, lab.Catalog, lab.Catalog), seatMaps);
    }

    public static async Task<AncillaryShoppingSnapshot> ShopOrderAsync(ShoppingLab lab, IOrderCommercialContextReader orders)
    {
        var (source, _) = await SourceAsync(lab, orders: orders);

        return await Offers(source, lab).ExecuteAsync(ByOrder, Sale);
    }

    public static string RefOf(AncillaryShoppingSnapshot shopping, string variantCode, params string[] travellerRefs)
        => shopping.Items.First(item => item.Candidate.VariantCode == variantCode && item.Candidate.Travellers.SequenceEqual(travellerRefs)).OfferItemRef;
}
