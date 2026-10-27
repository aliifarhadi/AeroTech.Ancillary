using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers
{
    public sealed class GetPricedOrderAncillaryOffersService : IGetPricedOrderAncillaryOffersService
    {
        private readonly ISelectionResolver _selections;
        private readonly IGetOrderAncillaryOffersService _offers;

        public GetPricedOrderAncillaryOffersService(ISelectionResolver selections, IGetOrderAncillaryOffersService offers)
        {
            _selections = selections;
            _offers = offers;
        }

        public async Task<OrderAncillaryOffersSnapshot> ExecuteAsync(string selectionId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => await _offers.ExecuteAsync(await _selections.OpenAsync(selectionId, ShoppingSourceKind.Order, cancellationToken), scope, cancellationToken);
    }
}
