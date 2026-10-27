using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public sealed record OrderShopping(OrderCommercialContext Order, FlightOfferShopping Shopping);
}
