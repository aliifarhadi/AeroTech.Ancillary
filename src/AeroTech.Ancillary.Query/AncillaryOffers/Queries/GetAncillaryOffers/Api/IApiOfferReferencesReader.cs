using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public interface IApiOfferReferencesReader
    {
        Task<ApiOfferReferences> ReadAsync(AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken = default);
    }
}
