using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class SelectionDesk
{
    public const string SigningKey = "ancillary-priced-selection-test-signing-key";

    public static PricedSelectionTokens Tokens(FixedClock clock, string? signingKey = SigningKey) => new(new PricedSelectionOptions(signingKey), clock);

    public static async Task<(EvaluateBatchService Service, GetSeatMapService SeatMaps)> BatchAsync(
        ShoppingLab lab,
        IOccupiedSeatProvider? occupied = null,
        FlightOffer? offer = null,
        FixedClock? clock = null,
        IOrderCommercialContextReader? orders = null)
    {
        var (source, airports) = await SourceAsync(lab, offer, orders);
        var offers = new GetAncillaryOffersService(source, lab.Engine);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var seatMaps = new GetSeatMapService(source, new FixedSeatMaps(Aircraft()), occupied ?? new FixedOccupiedSeats(), lab.Engine, lab.Catalog, airports, lab.Catalog);
        var service = new EvaluateBatchService(
            source,
            offers,
            details,
            new EvaluateSelectionService(details, lab.Engine),
            seatMaps,
            Tokens(clock ?? new FixedClock { Now = Sale.EvaluatedAtUtc }));

        return (service, seatMaps);
    }
}
