using AeroTech.Ancillary.Domain._Shared.Resources;
using System.Globalization;

namespace AeroTech.Ancillary.Application._Shared.Time
{
    public static class UtcInstant
    {
        private static readonly string[] Formats =
        [
            "yyyy-MM-dd'T'HH:mmK",
            "yyyy-MM-dd'T'HH:mm:ssK",
            "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"
        ];

        public static DateTimeOffset Parse(string? value, string field)
        {
            var text = value?.Trim() ?? string.Empty;
            var hasOffset = text.EndsWith('Z') || (text.Length > 6 && text[^6] is '+' or '-' && text[^3] == ':');

            return hasOffset && DateTimeOffset.TryParseExact(text, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var instant)
                ? instant.ToUniversalTime()
                : throw ExceptionFactory.InventorySourceIsInvalid(field);
        }

        public static DateTimeOffset? ParseOptional(string? value, string field)
            => string.IsNullOrWhiteSpace(value) ? null : Parse(value, field);
    }
}
