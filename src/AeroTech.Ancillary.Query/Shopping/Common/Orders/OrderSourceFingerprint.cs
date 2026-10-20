using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public static class OrderSourceFingerprint
    {
        private const char Separator = '\n';

        public static string Of(OrderCommercialContext order)
        {
            var parts = new List<string>
            {
                FlightOfferFingerprint.Of(order.Air),
                order.OrderId.ToString(CultureInfo.InvariantCulture),
                order.CommercialVersion.ToString(CultureInfo.InvariantCulture),
                order.TicketedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty
            };

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

            return Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(Separator, parts))));
        }
    }
}
