using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public sealed class GetGroupedAncillaryOffersService : IGetGroupedAncillaryOffersService
    {
        private readonly IGetAncillaryOffersService _offers;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public GetGroupedAncillaryOffersService(IGetAncillaryOffersService offers, IActiveAncillaryDefinitionReader definitions)
        {
            _offers = offers;
            _definitions = definitions;
        }

        public async Task<GroupedAncillaryOffersSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var shopping = await _offers.ExecuteAsync(query, scope, OfferFamilyFilters.Services, cancellationToken);
            var definitions = (await _definitions.ListActiveAsync(scope.OwnerAirlineId, cancellationToken)).ToDictionary(definition => definition.Id);
            var items = shopping.Items
                .Where(item => IsShown(item.Candidate.OfferReadiness))
                .OrderBy(item => item.Candidate.Profile)
                .ThenBy(item => item.Candidate.ServiceDefinitionRef, StringComparer.Ordinal)
                .ThenBy(item => item.PointOfSaleId)
                .Select(item => new GroupedOfferItem(
                    item,
                    definitions.GetValueOrDefault(item.Candidate.DefinitionVersionId) ?? throw ExceptionFactory.OfferItemWasNotFound(),
                    OfferInlineOptions.Interaction(item.Candidate, item.OptionGroups).RequiresDetails))
                .ToList();

            return new GroupedAncillaryOffersSnapshot(
                shopping,
                query.OrderId is null ? shopping.Offer.OfferId : null,
                query.OrderId,
                shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context.ShoppingStage).FirstOrDefault(FlightOfferShoppingContextMapper.Stage),
                shopping.Offer.Bounds
                    .OrderBy(bound => bound.Sequence)
                    .SelectMany(bound => bound.Flights.Select(flight => flight.FlightId))
                    .Distinct()
                    .ToList(),
                items);
        }

        public static bool IsShown(OfferReadiness readiness)
            => readiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection or OfferReadiness.NeedsQuote;
    }
}
