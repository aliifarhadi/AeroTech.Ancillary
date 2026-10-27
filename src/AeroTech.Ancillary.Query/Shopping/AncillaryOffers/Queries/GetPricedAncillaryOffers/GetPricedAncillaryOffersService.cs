using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers
{
    public sealed class GetPricedAncillaryOffersService : IGetPricedAncillaryOffersService
    {
        private readonly ISelectionResolver _selections;
        private readonly IGetAncillaryOffersService _offers;

        public GetPricedAncillaryOffersService(ISelectionResolver selections, IGetAncillaryOffersService offers)
        {
            _selections = selections;
            _offers = offers;
        }

        public async Task<AncillaryOffersSnapshot> ExecuteAsync(string selectionId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => await _offers.ExecuteAsync(await _selections.OpenAsync(selectionId, ShoppingSourceKind.Offer, cancellationToken), scope, cancellationToken);
    }
}
