using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public static class SelectionBindings
    {
        public const int Length = 43;

        private const string OfferDomain = "AERO-OFFER-v1";
        private const string SalesDomain = "AERO-SALE-v1";
        private const string Absent = "-";
        private const char Separator = '|';

        public static string Offer(string offerId, string trustedSourceVersion)
            => Digest(OfferDomain, Encode(offerId), Encode(trustedSourceVersion));

        public static string Sales(AncillaryShoppingContext context)
            => Digest(SalesDomain, Number(context.OwnerAirlineId), Number(context.PointOfSaleId), Optional(context.CustomerId));

        private static string Digest(params string[] parts)
            => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(Separator, parts))));

        private static string Encode(string value) => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(value));

        private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Optional(long? value) => value is { } number ? Number(number) : Absent;
    }
}
