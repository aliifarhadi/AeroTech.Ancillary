using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Options;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions
{
    public sealed class GetSelectionOptionsService : IGetSelectionOptionsService
    {
        private readonly IFlightOfferShoppingSource _source;
        private readonly ITypedSelectionService _typed;

        public GetSelectionOptionsService(IFlightOfferShoppingSource source, ITypedSelectionService typed)
        {
            _source = source;
            _typed = typed;
        }

        public async Task<SelectionOptionsForm> ExecuteAsync(ISelectionOptionsQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var family = OfferFamilyRoutes.ProfileOf(query.Family) ?? throw ExceptionFactory.SelectionWasNotFound("the family does not exist");
            var reference = await _typed.ReadReferenceAsync(query.Reference);
            var shopping = await _source.LoadAsync(query.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            return await _typed.FormAsync(reference, family, shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList(), cancellationToken);
        }
    }
}
