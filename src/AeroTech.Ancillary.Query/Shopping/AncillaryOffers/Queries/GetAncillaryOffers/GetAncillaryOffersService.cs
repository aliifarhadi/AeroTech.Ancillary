using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed class GetAncillaryOffersService : IGetAncillaryOffersService
    {
        private readonly IShoppingSource _source;
        private readonly IAncillaryShoppingEngine _engine;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IActiveAncillaryProvisionReader _provisions;

        public GetAncillaryOffersService(
            IShoppingSource source,
            IAncillaryShoppingEngine engine,
            IActiveAncillaryDefinitionReader definitions,
            IActiveAncillaryProvisionReader provisions)
        {
            _source = source;
            _engine = engine;
            _definitions = definitions;
            _provisions = provisions;
        }

        public Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ExecuteAsync(query, scope, ShoppingFilter.None, cancellationToken);

        public async Task<AncillaryShoppingSnapshot> ExecuteAsync(
            IAncillaryOffersQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
            => await ShopAsync(await _source.LoadAsync(query.OfferId, query.OrderId, scope, cancellationToken), scope, family, cancellationToken);

        public async Task<AncillaryShoppingSnapshot> ShopAsync(
            FlightOfferShopping shopping,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            var pointsOfSale = new List<PointOfSaleShopping>();
            var definitions = (await _definitions.ListActiveAsync(scope.OwnerAirlineId, cancellationToken)).ToDictionary(definition => definition.Id);

            foreach (var (pointOfSaleId, context) in shopping.PointsOfSale)
            {
                var result = await _engine.ShopAsync(context, family, cancellationToken);
                var definitionIds = result.Candidates.Select(candidate => candidate.DefinitionVersionId).Distinct().ToList();
                var provisions = definitionIds.Count == 0
                    ? new Dictionary<long, AncillaryProvision>()
                    : (await _provisions.ListActiveAsync(definitionIds, pointOfSaleId, cancellationToken)).ToDictionary(provision => provision.Id);

                pointsOfSale.Add(new PointOfSaleShopping(
                    pointOfSaleId,
                    context,
                    result,
                    result.Candidates
                        .Select(candidate => new AncillaryOfferItem(
                            OfferItemRef.Of(shopping.Offer.OfferId, context.SourceIdentity.TrustedSourceVersion, scope, pointOfSaleId, candidate.CandidateIdentity),
                            pointOfSaleId,
                            context,
                            candidate,
                            OptionGroups(candidate, context, definitions, provisions)))
                        .ToList()));
            }

            return new AncillaryShoppingSnapshot(shopping.Offer, scope, shopping.Facts, shopping.CurrencyId, shopping.EvaluatedAtUtc, pointsOfSale);
        }

        private static IReadOnlyList<OfferOptionGroup> OptionGroups(
            CanonicalAncillaryOfferCandidate candidate,
            AncillaryShoppingContext context,
            IReadOnlyDictionary<long, AncillaryServiceDefinition> definitions,
            IReadOnlyDictionary<long, AncillaryProvision> provisions)
            => definitions.TryGetValue(candidate.DefinitionVersionId, out var definition) && provisions.TryGetValue(candidate.ProvisionId, out var provision)
                ? OfferOptionCatalog.Build(definition, provision, candidate, context)
                : [];

        public Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default)
            => FindItemAsync(query, scope, ShoppingFilter.None, cancellationToken);

        public async Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.OfferItemRef))
                throw ExceptionFactory.OfferItemRefIsRequired();

            var shopping = await ExecuteAsync(query, scope, family, cancellationToken);
            var item = shopping.Items.SingleOrDefault(candidate => string.Equals(candidate.OfferItemRef, query.OfferItemRef, StringComparison.Ordinal))
                       ?? throw ExceptionFactory.OfferItemWasNotFound();

            return (shopping, item);
        }
    }
}
