using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2OfferScopeTests
{
    private const long CustomerA = 5001;
    private const long CustomerB = 5002;

    private static async Task<FlightOffer> OfferAsync(Channel channel, long? customerId)
        => (await OfferWire.ReadAsync(OfferWire.Captured())) with { SalesChannel = channel, CustomerId = customerId };

    private static AncillarySalesScope Agency(long? customerId) => new(Airline, AgencySalesPolicy.SalesChannel, [Pos], TravelAgencyId: 77, CustomerId: customerId);

    [Fact]
    public async Task SEC_07_an_agency_prices_only_its_own_customer_offer_and_never_one_bound_to_another_or_to_a_customer_it_does_not_carry()
    {
        var bound = await OfferAsync(AgencySalesPolicy.SalesChannel, CustomerA);
        var open = await OfferAsync(AgencySalesPolicy.SalesChannel, null);

        PricedOfferScopeGuard.EnsureSellable(bound, Agency(CustomerA));
        PricedOfferScopeGuard.EnsureSellable(open, Agency(null));

        foreach (var (offer, scope) in new[] { (bound, Agency(CustomerB)), (bound, Agency(null)), (open, Agency(CustomerA)) })
        {
            var refused = Assert.Throws<BusinessException>(() => PricedOfferScopeGuard.EnsureSellable(offer, scope));

            Assert.Equal((16809, 403), (refused.Code, refused.HttpStatus));
        }
    }

    [Fact]
    public async Task SEC_07_airline_staff_preview_a_customer_bound_offer_of_their_channel_but_never_another_channel()
    {
        var staff = new AncillarySalesScope(Airline, Channel.BackOffice, [Pos], AirlineOfficeId: BackofficeOfficeId);

        PricedOfferScopeGuard.EnsureSellable(await OfferAsync(Channel.BackOffice, CustomerA), staff);
        PricedOfferScopeGuard.EnsureSellable(await OfferAsync(Channel.BackOffice, null), staff);

        var refused = await Assert.ThrowsAsync<BusinessException>(async () => PricedOfferScopeGuard.EnsureSellable(await OfferAsync(AgencySalesPolicy.SalesChannel, CustomerA), staff));

        Assert.Equal((16809, 403), (refused.Code, refused.HttpStatus));
    }

    [Fact]
    public async Task VIEW_15_the_source_fingerprint_changes_with_every_sale_flight_leg_and_fare_fact_that_prices_or_qualifies_an_ancillary()
    {
        var offer = await OfferAsync(Channel.BackOffice, CustomerA);
        var baseline = FlightOfferFingerprint.Of(offer);
        var flight = offer.Bounds[0].Flights[0];
        var fare = offer.FareComponents[0];

        FlightOffer WithFlight(FlightOfferFlight changed)
            => offer with { Bounds = [offer.Bounds[0] with { Flights = [changed, .. offer.Bounds[0].Flights.Skip(1)] }, .. offer.Bounds.Skip(1)] };

        FlightOffer WithFare(FlightOfferFareComponent changed)
            => offer with { FareComponents = [changed, .. offer.FareComponents.Skip(1)] };

        var leg = new FlightOfferLeg(1, 9001, flight.OriginAirportId, flight.DestinationAirportId, flight.DepartureAt, flight.ArrivalAt);
        var changes = new[]
        {
            offer with { CustomerId = CustomerB },
            offer with { SalesChannel = AgencySalesPolicy.SalesChannel },
            offer with { SalesCountryId = (offer.SalesCountryId ?? 0) + 1 },
            offer with { Bounds = [offer.Bounds[0] with { Sequence = offer.Bounds[0].Sequence + 10 }, .. offer.Bounds.Skip(1)] },
            WithFlight(flight with { FlightNumber = flight.FlightNumber + "9" }),
            WithFlight(flight with { MarketingAirlineId = flight.MarketingAirlineId + 1 }),
            WithFlight(flight with { OperatingAirlineId = flight.OperatingAirlineId + 1 }),
            WithFlight(flight with { OriginAirportTerminalId = (flight.OriginAirportTerminalId ?? 0) + 1 }),
            WithFlight(flight with { Legs = [.. flight.Legs, leg] }),
            WithFlight(flight with { Legs = [.. flight.Legs, leg with { LegId = 9002 }] }),
            WithFare(fare with { CabinClassId = (fare.CabinClassId ?? 0) + 1 }),
            WithFare(fare with { RbdId = (fare.RbdId ?? 0) + 1 }),
            WithFare(fare with { BookingClass = fare.BookingClass + "Z" })
        };
        var fingerprints = changes.Select(FlightOfferFingerprint.Of).ToList();

        Assert.Equal(baseline, FlightOfferFingerprint.Of(offer with { PricedAt = offer.PricedAt.ToOffset(TimeSpan.FromHours(5)) }));
        Assert.DoesNotContain(baseline, fingerprints);
        Assert.Equal(fingerprints.Count, fingerprints.Distinct(StringComparer.Ordinal).Count());
    }
}
