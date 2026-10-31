using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.VerifySeatPrices
{
    public sealed class SeatPriceVerifier : ISeatPriceVerifier
    {
        private const string IdsMember = "ids";
        private const int SingleSeat = 1;

        private readonly IBackofficeSalesScopeResolver _backoffice;
        private readonly IOtaPanelSalesScopeResolver _otaPanel;
        private readonly IApiSalesScopeResolver _api;
        private readonly IFlightOfferShoppingSource _source;
        private readonly IOrderShoppingSource _orders;
        private readonly IGetSeatMapService _seatMaps;
        private readonly SelectionIdOptions _options;
        private readonly IClock _clock;

        public SeatPriceVerifier(
            IBackofficeSalesScopeResolver backoffice,
            IOtaPanelSalesScopeResolver otaPanel,
            IApiSalesScopeResolver api,
            IFlightOfferShoppingSource source,
            IOrderShoppingSource orders,
            IGetSeatMapService seatMaps,
            SelectionIdOptions options,
            IClock clock)
        {
            _backoffice = backoffice;
            _otaPanel = otaPanel;
            _api = api;
            _source = source;
            _orders = orders;
            _seatMaps = seatMaps;
            _options = options;
            _clock = clock;
        }

        public async Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SeatPriceVerification verification, CancellationToken cancellationToken = default)
        {
            var selections = Decoded(verification.Ids);
            var scope = await ScopeAsync(verification.Surface, selections, cancellationToken);
            var shopping = await _source.LoadAsync(verification.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            return await ProveAllAsync(verification.Ids, shopping, scope, cancellationToken);
        }

        public async Task<VerifiedOrderSelections> VerifyAsync(OrderSeatPriceVerification verification, CancellationToken cancellationToken = default)
        {
            var selections = Decoded(verification.Ids);
            var scope = await ScopeAsync(verification.Surface, selections, cancellationToken);
            var order = await _orders.LoadAsync(verification.OrderId, scope, cancellationToken);

            return new VerifiedOrderSelections(
                order.Order.CommercialVersion,
                await ProveAllAsync(verification.Ids, order.Shopping, scope, cancellationToken));
        }

        private static IReadOnlyList<SeatMapSelection> Decoded(IReadOnlyList<string> ids)
        {
            var selections = ids
                .Select(id => SeatMapSelectionId.Decode(id) ?? throw ExceptionFactory.ShoppingContextIsInvalid($"{id} is not a seat map id"))
                .ToList();

            if (selections.GroupBy(seat => (seat.FlightId, seat.SeatNumber)).Any(group => group.Skip(1).Any()))
                throw ExceptionFactory.ShoppingContextIsInvalid("a seat of a flight must be selected once");

            return selections.GroupBy(seat => (seat.FlightId, seat.TravellerRef)).Any(group => group.Skip(1).Any())
                ? throw ExceptionFactory.ShoppingContextIsInvalid("a traveller must select one seat per flight")
                : selections;
        }

        private Task<AncillarySalesScope> ScopeAsync(AuthorizationSurface surface, IReadOnlyList<SeatMapSelection> selections, CancellationToken cancellationToken)
            => surface switch
            {
                AuthorizationSurface.Backoffice => _backoffice.ResolveForPointsOfSaleAsync(selections.Select(seat => seat.PointOfSaleId).Distinct().Order().ToList(), cancellationToken),
                AuthorizationSurface.OtaPanel => _otaPanel.ResolveAsync(cancellationToken),
                AuthorizationSurface.Api => _api.ResolveAsync(cancellationToken),
                _ => throw ExceptionFactory.ShoppingCallerNotAuthorized()
            };

        private async Task<IReadOnlyList<VerifiedSelection>> ProveAllAsync(
            IReadOnlyList<string> ids,
            FlightOfferShopping shopping,
            AncillarySalesScope scope,
            CancellationToken cancellationToken)
        {
            var seatMap = await _seatMaps.BuildAsync(shopping, scope, cancellationToken);
            var offered = seatMap.Flights
                .SelectMany(flight => flight.Offers.Where(offer => offer.Id is not null).Select(offer => (flight.FlightRef, Offer: offer)))
                .ToDictionary(seat => seat.Offer.Id!, StringComparer.Ordinal);
            var expiresAtUtc = _clock.GetDateTime() + _options.MaxAge;
            var verified = new List<VerifiedSelection>(ids.Count);

            for (var index = 0; index < ids.Count; index++)
                verified.Add(await SelectionBatch.ItemAsync(IdsMember, index, () => Task.FromResult(Prove(ids[index], offered, seatMap, expiresAtUtc))));

            return verified;
        }

        private static VerifiedSelection Prove(
            string id,
            IReadOnlyDictionary<string, (string FlightRef, SeatTravellerOffer Offer)> offered,
            SeatMapSnapshot seatMap,
            DateTimeOffset expiresAtUtc)
        {
            if (!offered.TryGetValue(id, out var seat)
                || seat.Offer is not { Status: SeatAvailabilityStatus.Available, Price.PricingRateId: { } pricingRateId })
                throw ExceptionFactory.SelectionIsNotSellable("the seat is no longer available at this price; ask for the seat map again");

            var context = seatMap.PointsOfSale.First(pointOfSale => pointOfSale.PointOfSaleId == seat.Offer.PointOfSaleId).Context;
            var sourceBinding = SelectionBindings.Source(context.SourceIdentity)
                                ?? throw ExceptionFactory.SelectionIsNotSellable("the offer or order does not match the selection; ask for the seat map again");

            return new VerifiedSelection(
                pricingRateId,
                ServiceCoverageScope.Sector,
                seat.Offer.TravellerRef,
                seat.FlightRef,
                sourceBinding,
                SelectionBindings.Sales(context),
                SingleSeat,
                expiresAtUtc);
        }
    }
}
