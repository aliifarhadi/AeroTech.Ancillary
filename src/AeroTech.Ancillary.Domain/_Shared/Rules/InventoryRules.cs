using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain._Shared.Rules
{
    internal static class InventoryRules
    {
        public const decimal MaxKg = 999_999_999_999_999.999m;
        public const int KgScale = 3;
        public const int ReasonCodeMaxLength = 50;
        public const int CorrelationIdMaxLength = 64;

        public static bool IsKg(decimal value) => value >= 0m && value <= MaxKg && decimal.Round(value, KgScale) == value;

        public static bool IsCode(string? value, int maxLength)
            => value is { Length: >= 1 } && value.Length <= maxLength && !value.Any(char.IsWhiteSpace);

        public static void EnsureVerified(InventoryReferenceCheck check, string reference)
        {
            if (check == InventoryReferenceCheck.SourceUnavailable)
                throw ExceptionFactory.InventoryReferenceSourceUnavailable(reference);

            if (check != InventoryReferenceCheck.Verified)
                throw ExceptionFactory.InventoryReferenceNotFound(reference);
        }
    }
}
