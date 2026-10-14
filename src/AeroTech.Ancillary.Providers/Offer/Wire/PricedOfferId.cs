using System.Globalization;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public static class PricedOfferId
    {
        private const int SectionCount = 3;
        private const int CommercialPartCount = 5;
        private const int CustomerPart = 1;

        public static bool TryReadCustomerId(string offerId, out long? customerId)
        {
            customerId = null;
            var sections = offerId.Split(';');

            if (sections.Length != SectionCount)
                return false;

            var parts = sections[SectionCount - 1].Split('|');

            if (parts.Length != CommercialPartCount)
                return false;

            if (parts[CustomerPart].Length == 0)
                return true;

            if (!long.TryParse(parts[CustomerPart], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                return false;

            customerId = parsed;
            return true;
        }
    }
}
