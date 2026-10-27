using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public static class OrderScopeGuard
    {
        public static void EnsureSellable(OrderCommercialContext order, AncillarySalesScope scope)
        {
            if (scope.TravelAgencyId is { } travelAgencyId && order.TravelAgencyId != travelAgencyId)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            if (!scope.SellsForAnyCustomer && order.Air.CustomerId != scope.CustomerId)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();
        }
    }
}
