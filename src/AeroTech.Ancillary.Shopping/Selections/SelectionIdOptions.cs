using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed record SelectionIdOptions(string? SigningKey, TimeSpan MaxAge)
    {
        public const string SigningKeyKey = "Shopping:SelectionSigningKey";
        public const string MaxAgeKey = "Shopping:SelectionMaxAge";

        public static SelectionIdOptions From(IConfiguration configuration)
            => configuration[MaxAgeKey] is { } text
               && TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var maxAge)
               && maxAge > TimeSpan.Zero
                ? new SelectionIdOptions(configuration[SigningKeyKey], maxAge)
                : throw new InvalidOperationException($"'{MaxAgeKey}' must be configured as a positive time span.");
    }
}
