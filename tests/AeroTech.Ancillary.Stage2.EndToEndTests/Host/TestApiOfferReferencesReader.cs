using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class TestApiOfferReferencesReader(InMemoryCatalog catalog, IAirportReference airports) : IApiOfferReferencesReader
{
    public async Task<ApiOfferReferences> ReadAsync(AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken = default)
    {
        var flights = shopping.Offer.Bounds.SelectMany(bound => bound.Flights).ToList();
        var airportIds = flights.SelectMany(flight => flight.Legs.SelectMany(leg => new[] { leg.OriginAirportId, leg.DestinationAirportId })).Distinct().ToList();
        var airlineIds = flights.SelectMany(flight => new[] { flight.MarketingAirlineId, flight.OperatingAirlineId }).Distinct().ToList();
        var baggage = shopping.Items.Any(item => item.Candidate.Profile == AncillaryProfile.Baggage)
            ? (await catalog.ListActiveAsync(shopping.Scope.OwnerAirlineId, cancellationToken))
                .Where(definition => definition.Baggage is not null)
                .ToDictionary(definition => definition.Id, definition => definition.Baggage!.ToArgs())
            : new Dictionary<long, BaggageSpecificationArgs>();

        return new ApiOfferReferences(
            shopping.Offer,
            await airports.FindIataCodesAsync(airportIds, cancellationToken),
            await catalog.FindIataCodesAsync(airlineIds, cancellationToken),
            catalog.CurrencyCodes,
            baggage);
    }
}
