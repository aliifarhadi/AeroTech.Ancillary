using System.Globalization;
using System.Reflection;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public static class SelectionCanonicalForm
    {
        private static readonly PropertyInfo[] Fields = typeof(AncillarySelection)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        public static string Of(AncillarySelection selection)
            => string.Join('\n', Fields.Select(field => $"{field.Name}={Value(field.GetValue(selection))}"));

        private static string Value(object? value)
            => value switch
            {
                null => string.Empty,
                string text => text,
                decimal amount => amount.ToString("0.############################", CultureInfo.InvariantCulture),
                DateTimeOffset instant => instant.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                SelectedDimensions size => string.Join(',', Value(size.LengthCm), Value(size.WidthCm), Value(size.HeightCm)),
                IEnumerable<string> references => string.Join(',', references.Order(StringComparer.Ordinal)),
                Enum code => Convert.ToInt64(code, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
    }
}
