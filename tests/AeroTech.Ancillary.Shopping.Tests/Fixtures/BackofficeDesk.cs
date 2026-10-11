using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.FlightOffers;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class BackofficeDesk
{
    public static readonly FlightOfferSalesScope Sale = new(Airline, Pos, null, Eur, Now);

    public static async Task<AncillaryOffersDto> ShopAsync(
        ShoppingLab lab,
        string? wire = null,
        TestOnlyAirportReference? airports = null,
        FlightOfferSalesScope? scope = null)
        => await new GetAncillaryOffersService(new FixedOffers(await OfferWire.ReadAsync(wire ?? OfferWire.Captured())), airports ?? new TestOnlyAirportReference(), lab.Engine)
            .ExecuteAsync(new OfferQuery(OfferWire.OfferId), scope ?? Sale);

    public static BackofficeSalesScopeResolver Resolver(Caller? caller = null, HomeAirline? home = null, Eligibility? eligibility = null, FixedClock? clock = null)
        => new(caller ?? new Caller(), home ?? new HomeAirline(), eligibility ?? new Eligibility(), clock ?? new FixedClock());

    public sealed record OfferQuery(string? OfferId) : IAncillaryOffersQuery;

    public sealed class FixedOffers : IFlightOfferProvider
    {
        private readonly FlightOffer _offer;

        public FixedOffers(FlightOffer offer) => _offer = offer;

        public Task<FlightOffer> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default) => Task.FromResult(_offer);
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
}
