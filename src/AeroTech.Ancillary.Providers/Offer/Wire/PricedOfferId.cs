using System.Globalization;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public static class PricedOfferId
    {
        private const int SectionCount = 3;
        private const int CommercialPartCount = 5;
        private const int ChannelPart = 0;
        private const int CustomerPart = 1;
        private const int CountryPart = 2;
        private const int CurrencyPart = 3;

        private static readonly IReadOnlyDictionary<int, Channel> ProducerChannels = new Dictionary<int, Channel>
        {
            [1] = Channel.BackOffice,
            [2] = Channel.IBE,
            [3] = Channel.PartnerAPI,
            [4] = Channel.AgencyPanel,
            [5] = Channel.GDS,
            [99] = Channel.System
        };

        public static bool TryRead(string offerId, out PricedOfferCommercial? commercial)
        {
            commercial = null;
            var sections = offerId.Split(';');

            if (sections.Length != SectionCount)
                return false;

            var parts = sections[SectionCount - 1].Split('|');

            if (parts.Length != CommercialPartCount
                || !int.TryParse(parts[ChannelPart], NumberStyles.None, CultureInfo.InvariantCulture, out var channel)
                || !TryReadOptional(parts[CustomerPart], out var customerId)
                || !TryReadOptional(parts[CountryPart], out var countryId)
                || !int.TryParse(parts[CurrencyPart], NumberStyles.None, CultureInfo.InvariantCulture, out var currencyId))
                return false;

            commercial = new PricedOfferCommercial(
                ProducerChannels.TryGetValue(channel, out var known) ? known : null,
                customerId,
                countryId,
                currencyId);

            return true;
        }

        private static bool TryReadOptional(string part, out long? value)
        {
            value = null;

            if (part.Length == 0)
                return true;

            if (!long.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                return false;

            value = parsed;

            return true;
        }
    }
}
