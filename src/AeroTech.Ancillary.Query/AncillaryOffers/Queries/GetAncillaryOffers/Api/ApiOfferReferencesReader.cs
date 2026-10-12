using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiOfferReferencesReader : IApiOfferReferencesReader
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public ApiOfferReferencesReader(AncillaryQueryDbContext dbContext, IActiveAncillaryDefinitionReader definitions)
        {
            _dbContext = dbContext;
            _definitions = definitions;
        }

        public async Task<ApiOfferReferences> ReadAsync(AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken = default)
        {
            var flights = shopping.Offer.Bounds.SelectMany(bound => bound.Flights).ToList();
            var airportIds = flights
                .SelectMany(flight => flight.Legs.SelectMany(leg => new[] { leg.OriginAirportId, leg.DestinationAirportId }))
                .Distinct()
                .ToList();
            var airlineIds = flights.SelectMany(flight => new[] { flight.MarketingAirlineId, flight.OperatingAirlineId }).Distinct().ToList();
            var airports = await _dbContext.Airports
                .AsNoTracking()
                .Where(airport => airportIds.Contains(airport.Id))
                .ToDictionaryAsync(airport => airport.Id, airport => airport.IataCode, cancellationToken);
            var airlines = await _dbContext.Airlines
                .AsNoTracking()
                .Where(airline => airlineIds.Contains(airline.Id))
                .ToDictionaryAsync(airline => airline.Id, airline => airline.IataCode, cancellationToken);
            var currencies = await _dbContext.Currencies
                .AsNoTracking()
                .ToDictionaryAsync(currency => currency.Id, currency => currency.Code, cancellationToken);
            var baggage = shopping.Items.Any(item => item.Candidate.Profile == AncillaryProfile.Baggage)
                ? (await _definitions.ListActiveAsync(shopping.Scope.OwnerAirlineId, cancellationToken))
                    .Where(definition => definition.Baggage is not null)
                    .ToDictionary(definition => definition.Id, definition => definition.Baggage!.ToArgs())
                : new Dictionary<long, BaggageSpecificationArgs>();

            return new ApiOfferReferences(shopping.Offer, airports, airlines, currencies, baggage);
        }
    }
}
