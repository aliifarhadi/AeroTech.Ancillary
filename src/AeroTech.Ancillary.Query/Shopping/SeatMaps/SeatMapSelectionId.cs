using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps
{
    public static class SeatMapSelectionId
    {
        private const char Separator = '|';
        private const int PartCount = 5;
        private const int MaximumLength = 256;

        public static string Encode(string travellerRef, long flightId, long pricingId, string seatNumber, long pointOfSaleId)
            => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(string.Join(
                Separator,
                travellerRef,
                flightId.ToString(CultureInfo.InvariantCulture),
                pricingId.ToString(CultureInfo.InvariantCulture),
                seatNumber.Trim().ToUpperInvariant(),
                pointOfSaleId.ToString(CultureInfo.InvariantCulture))));

        public static SeatMapSelection? Decode(string? id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > MaximumLength || !Base64Url.IsValid(id))
                return null;

            var parts = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(id)).Split(Separator);

            return parts.Length == PartCount
                   && parts[0].Length > 0
                   && long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var flightId)
                   && long.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var pricingId)
                   && parts[3].Length > 0
                   && long.TryParse(parts[4], NumberStyles.None, CultureInfo.InvariantCulture, out var pointOfSaleId)
                ? new SeatMapSelection(parts[0], flightId, pricingId, parts[3], pointOfSaleId)
                : null;
        }
    }
}
