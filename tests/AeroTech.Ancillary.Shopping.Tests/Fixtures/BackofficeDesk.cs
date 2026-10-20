using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Core.Enums;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class BackofficeDesk
{
    public const long BackofficeOfficeId = 9200;

    public static readonly DeskScope Sale = new(Airline, Pos, null, Eur, Now);

    public static readonly TimeSpan SourceContextTtl = TimeSpan.FromMinutes(30);

    public static ShoppingSource Sources(IFlightOfferShoppingSource flightOffers, IOrderCommercialContextReader? orders = null)
        => new(flightOffers, new OrderShoppingSource(orders ?? new OrderDesk.FixedOrders(), flightOffers));

    public static async Task<GetAncillaryOffersService> ServiceAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        TestOnlyAirFareReference? airFares = null,
        DeskScope? desk = null,
        Channel channel = Channel.BackOffice)
        => new(
            Sources(new FlightOfferShoppingSource(
                new FixedOffers((desk ?? Sale).Sell(await OfferWire.ReadAsync(wire ?? OfferWire.Captured()), channel)),
                airports ?? new TestOnlyAirportReference(),
                airFares ?? new TestOnlyAirFareReference(),
                new FixedClock { Now = (desk ?? Sale).EvaluatedAtUtc },
                new FlightOfferShoppingOptions(SourceContextTtl))),
            lab.Engine);

    public static async Task<AncillaryShoppingSnapshot> SnapshotAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        DeskSale? scope = null,
        TestOnlyAirFareReference? airFares = null)
    {
        var sale = scope ?? Sale;

        return await (await ServiceAsync(lab, wire, airports, airFares, sale.Desk, sale.Scope.Channel)).ExecuteAsync(new OfferQuery(OfferWire.OfferId), sale.Scope);
    }

    public static async Task<BackofficeAncillaryOffersDto> ShopAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        DeskScope? scope = null,
        TestOnlyAirFareReference? airFares = null)
        => BackofficeAncillaryOffersMapper.Map(await SnapshotAsync(lab, wire, airports, scope ?? Sale, airFares));

    public static AncillaryShoppingContext ContextOf(FlightOffer offer, DeskScope scope, IReadOnlyDictionary<int, int> airportCountryIds)
    {
        var sold = scope.Sell(offer);

        return FlightOfferShoppingContextMapper.Map(
            sold,
            FlightOfferShoppingContextMapper.Facts(sold, new Dictionary<long, long>()),
            scope,
            scope.PointOfSaleId,
            airportCountryIds,
            new Dictionary<int, string>(),
            scope.EvaluatedAtUtc,
            scope.EvaluatedAtUtc + SourceContextTtl,
            FlightOfferFingerprint.Of(sold));
    }

    public static BackofficeSalesScopeResolver Resolver(Caller? caller = null, HomeAirline? home = null, Eligibility? eligibility = null)
        => new(caller ?? new Caller(), home ?? new HomeAirline(), eligibility ?? new Eligibility());

    public sealed record DeskScope(int OwnerAirlineId, long PointOfSaleId, long? CustomerId, int? CurrencyId, DateTimeOffset EvaluatedAtUtc)
    {
        public DeskSale On(Channel channel, params long[] pointOfSaleIds)
            => new(
                this,
                new AncillarySalesScope(
                    OwnerAirlineId,
                    channel,
                    pointOfSaleIds,
                    AirlineOfficeId: channel == Channel.BackOffice ? BackofficeOfficeId : null,
                    CustomerId: CustomerId));

        public FlightOffer Sell(FlightOffer offer, Channel channel = Channel.BackOffice)
            => offer with
            {
                CustomerId = CustomerId,
                CurrencyId = CurrencyId ?? offer.CurrencyId,
                CurrencyCode = CurrencyId is { } currencyId && currencyId != offer.CurrencyId ? null : offer.CurrencyCode,
                PricedAt = EvaluatedAtUtc,
                SalesChannel = channel
            };

        public static implicit operator DeskSale(DeskScope scope) => scope.On(Channel.BackOffice, scope.PointOfSaleId);

        public static implicit operator AncillarySalesScope(DeskScope scope) => scope.On(Channel.BackOffice, scope.PointOfSaleId).Scope;
    }

    public sealed record DeskSale(DeskScope Desk, AncillarySalesScope Scope)
    {
        public static implicit operator AncillarySalesScope(DeskSale sale) => sale.Scope;
    }

    public sealed record OfferQuery(string? OfferId, string? OfferItemRef = null, long? OrderId = null) : IAncillaryOfferItemQuery;

    public sealed class FixedOffers : IFlightOfferProvider
    {
        private readonly FlightOffer _offer;

        public FixedOffers(FlightOffer offer) => _offer = offer;

        public int Calls { get; private set; }

        public Task<FlightOffer> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default)
        {
            Calls++;

            return Task.FromResult(_offer);
        }
    }

    public sealed class Caller : ICallerContext
    {
        public CallerContextType ContextType { get; init; } = CallerContextType.Airline;

        public CallerPrincipalType PrincipalType { get; init; } = CallerPrincipalType.Human;

        public long? AirlineUserId { get; init; } = 7001;

        public long? AirlineOfficeId { get; init; } = BackofficeOfficeId;

        public long? TravelAgencyUserId { get; init; }

        public long? TravelAgencyId { get; init; }

        public IReadOnlyCollection<long> TravelAgencyOfficeIds { get; init; } = [];

        public long? IndividualId { get; init; }

        public long? PartnerApiAccessProfileId { get; init; }

        public long? CustomerId { get; init; }

        public long ActorId => AirlineUserId ?? 0;
    }

    public sealed class HomeAirline : IOperatorAirlineResolver
    {
        public long? AirlineId { get; init; } = Airline;

        public Task<long?> FindHomeAirlineIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(AirlineId);
    }

    public sealed class Eligibility : IPointOfSaleEligibilityReference
    {
        public InventoryReferenceCheck Result { get; init; } = InventoryReferenceCheck.SourceUnavailable;

        public long[] Allowed { get; init; } = [];

        public PointOfSaleEligibilityQuery? Asked { get; private set; }

        public Task<PointOfSaleEligibility> ResolveAsync(PointOfSaleEligibilityQuery query, CancellationToken cancellationToken = default)
        {
            Asked = query;

            return Task.FromResult(new PointOfSaleEligibility(Result, Allowed));
        }
    }

    public sealed class Country : IRequestCountry
    {
        public long? CountryId { get; init; }
    }
}
