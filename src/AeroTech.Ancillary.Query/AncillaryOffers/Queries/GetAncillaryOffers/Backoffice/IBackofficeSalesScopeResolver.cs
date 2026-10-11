using AeroTech.Ancillary.Shopping.FlightOffers;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public interface IBackofficeSalesScopeResolver
    {
        Task<FlightOfferSalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default);
    }
}
