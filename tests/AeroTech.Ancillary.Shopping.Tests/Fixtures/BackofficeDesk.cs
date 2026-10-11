using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Core.Enums;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class BackofficeDesk
{
    public static readonly DeskScope Sale = new(Airline, Pos, null, Eur, Now);

    public static async Task<GetAncillaryOffersService> ServiceAsync(ShoppingLab lab, string? wire = null, TestOnlyAirportReference? airports = null)
        => new(new FixedOffers(await OfferWire.ReadAsync(wire ?? OfferWire.Captured())), airports ?? new TestOnlyAirportReference(), lab.Engine);

    public static async Task<AncillaryShoppingSnapshot> SnapshotAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        AncillarySalesScope? scope = null)
        => await (await ServiceAsync(lab, wire, airports)).ExecuteAsync(new OfferQuery(OfferWire.OfferId), scope ?? (AncillarySalesScope)Sale);

    public static async Task<BackofficeAncillaryOffersDto> ShopAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        DeskScope? scope = null)
        => BackofficeAncillaryOffersMapper.Map(await SnapshotAsync(lab, wire, airports, scope ?? Sale));

    public static AncillaryShoppingContext ContextOf(FlightOffer offer, DeskScope scope, IReadOnlyDictionary<int, int> airportCountryIds)
        => FlightOfferShoppingContextMapper.Map(offer, FlightOfferShoppingContextMapper.Facts(offer), scope, scope.PointOfSaleId, airportCountryIds);

    public static BackofficeSalesScopeResolver Resolver(Caller? caller = null, HomeAirline? home = null, Eligibility? eligibility = null, FixedClock? clock = null)
        => new(caller ?? new Caller(), home ?? new HomeAirline(), eligibility ?? new Eligibility(), clock ?? new FixedClock());

    public sealed record DeskScope(int OwnerAirlineId, long PointOfSaleId, long? CustomerId, int? PreferredCurrencyId, DateTimeOffset EvaluatedAtUtc)
    {
        public static DeskScope Of(AncillarySalesScope scope)
            => new(scope.OwnerAirlineId, scope.PointOfSaleIds.Single(), scope.CustomerId, scope.PreferredCurrencyId, scope.SalesInstantUtc);

        public AncillarySalesScope On(Channel channel, params long[] pointOfSaleIds)
            => new(OwnerAirlineId, channel, pointOfSaleIds, CustomerId, PreferredCurrencyId, EvaluatedAtUtc);

        public static implicit operator AncillarySalesScope(DeskScope scope) => scope.On(Channel.BackOffice, scope.PointOfSaleId);
    }

    public sealed record OfferQuery(string? OfferId, string? OfferItemRef = null) : IAncillaryOfferItemQuery;

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

        public long? AirlineOfficeId { get; init; } = 9200;

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
