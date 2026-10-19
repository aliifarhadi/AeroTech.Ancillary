using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public static class PricedOfferScopeGuard
    {
        public static void EnsureSellable(FlightOffer offer, AncillarySalesScope scope)
        {
            if (offer.SalesChannel != scope.Channel)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            if (scope.CustomerId is { } customerId && offer.CustomerId != customerId)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            if (scope.CountryId is { } countryId && offer.SalesCountryId != countryId)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();
        }
    }
}
