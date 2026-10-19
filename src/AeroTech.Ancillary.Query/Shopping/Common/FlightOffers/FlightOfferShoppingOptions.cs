using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public sealed record FlightOfferShoppingOptions(TimeSpan SourceContextTtl)
    {
        public const string SourceContextTtlKey = "Shopping:SourceContextTtl";

        public static FlightOfferShoppingOptions From(IConfiguration configuration)
            => configuration[SourceContextTtlKey] is { } text
               && TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var ttl)
               && ttl > TimeSpan.Zero
                ? new FlightOfferShoppingOptions(ttl)
                : throw new InvalidOperationException($"'{SourceContextTtlKey}' must be configured as a positive time span.");
    }
}
