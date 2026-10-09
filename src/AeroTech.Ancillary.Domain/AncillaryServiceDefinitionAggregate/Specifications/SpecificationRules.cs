using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    internal static class SpecificationRules
    {
        private const int DecimalScale = 3;
        private const decimal UpperBound = 1000000m;

        public static void Require(bool condition, string specification, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceDefinitionIsInvalid($"{specification}.{field}");
        }

        public static bool IsMeasure(decimal? value)
            => value is null || (value > 0 && value < UpperBound && value == decimal.Round(value.Value, DecimalScale));

        public static bool IsCode(string? value, int maxLength)
            => value is { Length: >= 1 } && value.Length <= maxLength && value.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');

        public static bool IsText(string? value, int maxLength)
            => value is null || (value.Length >= 1 && value.Length <= maxLength && value == value.Trim());

        public static List<SpecificationCode> Codes(IReadOnlyList<string>? values, int maxLength, string specification, string field)
        {
            var normalized = (values ?? []).Select(value => (value ?? string.Empty).Trim().ToUpperInvariant()).ToList();

            Require(normalized.All(value => IsCode(value, maxLength)), specification, field);
            Require(normalized.Distinct(StringComparer.Ordinal).Count() == normalized.Count, specification, field);

            return normalized.Select(value => new SpecificationCode(value)).ToList();
        }

        public static List<SpecificationReference> References(IEnumerable<long>? values, string specification, string field)
        {
            var listed = (values ?? []).ToList();

            Require(listed.All(value => value > 0), specification, field);
            Require(listed.Distinct().Count() == listed.Count, specification, field);

            return listed.Select(value => new SpecificationReference(value)).ToList();
        }
    }
}
