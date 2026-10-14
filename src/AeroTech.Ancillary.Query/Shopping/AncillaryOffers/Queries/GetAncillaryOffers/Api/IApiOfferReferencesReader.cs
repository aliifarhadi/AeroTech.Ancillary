using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public interface IApiOfferReferencesReader
    {
        Task<ApiOfferReferences> ReadAsync(AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken = default);
    }
}
