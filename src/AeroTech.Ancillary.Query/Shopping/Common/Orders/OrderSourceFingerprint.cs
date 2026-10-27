using System.Globalization;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public static class OrderSourceFingerprint
    {
        public static string Of(OrderCommercialContext order)
        {
            var parts = FlightOfferFingerprint.Parts(order.Air, includePrices: false);

            parts.AddRange(
            [
                order.OrderId.ToString(CultureInfo.InvariantCulture),
                order.CommercialVersion.ToString(CultureInfo.InvariantCulture),
                order.TicketedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                order.TravelAgencyId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
            ]);

            parts.AddRange(order.Travellers
                .OrderBy(traveller => traveller.TravellerRef, StringComparer.Ordinal)
                .Select(traveller => string.Join(
                    ',',
                    traveller.TravellerRef,
                    traveller.OrderTravellerId.ToString(CultureInfo.InvariantCulture),
                    traveller.DateOfBirth?.ToString("O", CultureInfo.InvariantCulture),
                    traveller.InfantOfTravellerRef)));
            parts.AddRange(order.Services
                .OrderBy(service => service.OrderServiceId)
                .Select(service => string.Join(
                    ',',
                    service.OrderServiceId.ToString(CultureInfo.InvariantCulture),
                    service.TravellerRef,
                    service.ServiceRef,
                    service.PortionRef,
                    string.Join('|', service.FlightRefs),
                    service.Quantity.ToString(CultureInfo.InvariantCulture),
                    service.CommercialState.ToString())));

            return FlightOfferFingerprint.Hash(parts);
        }
    }
}
