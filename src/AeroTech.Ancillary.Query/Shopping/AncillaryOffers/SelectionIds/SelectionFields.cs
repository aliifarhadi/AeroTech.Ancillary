using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds
{
    public static class SelectionFields
    {
        private const string Instant = "O";

        public static readonly IReadOnlyList<PropertyInfo> All = typeof(AncillarySelection)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToList();

        public static string CamelCase(string name) => string.Concat(char.ToLowerInvariant(name[0]), name[1..]);

        public static PropertyInfo? Find(string name) => All.FirstOrDefault(property => string.Equals(property.Name, name, StringComparison.Ordinal));

        public static bool IsSet(PropertyInfo field, AncillarySelection selection) => field.GetValue(selection) is { } value && value is not false;

        public static void Write(Utf8JsonWriter writer, AncillarySelection selection)
        {
            writer.WriteStartObject();

            foreach (var field in All.Where(field => IsSet(field, selection)))
            {
                writer.WritePropertyName(CamelCase(field.Name));
                WriteValue(writer, field.GetValue(selection)!);
            }

            writer.WriteEndObject();
        }

        public static AncillarySelection? Read(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            var selection = new AncillarySelection();

            foreach (var member in element.EnumerateObject())
            {
                var field = All.FirstOrDefault(property => string.Equals(CamelCase(property.Name), member.Name, StringComparison.Ordinal));

                if (field is null || ReadValue(field.PropertyType, member.Value) is not { } value)
                    return null;

                field.SetValue(selection, value);
            }

            return selection;
        }

        public static object? Parse(PropertyInfo field, string text)
        {
            var type = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;

            if (type == typeof(string))
                return text;

            if (type == typeof(int))
                return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : null;

            if (type == typeof(long))
                return long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : null;

            if (type.IsEnum)
                return Enum.TryParse(type, text, false, out var code) && Enum.IsDefined(type, code) ? code : null;

            return null;
        }

        private static void WriteValue(Utf8JsonWriter writer, object value)
        {
            switch (value)
            {
                case string text:
                    writer.WriteStringValue(text);
                    break;
                case bool flag:
                    writer.WriteBooleanValue(flag);
                    break;
                case int number:
                    writer.WriteNumberValue(number);
                    break;
                case long number:
                    writer.WriteNumberValue(number);
                    break;
                case decimal amount:
                    writer.WriteRawValue(amount.ToString("0.############################", CultureInfo.InvariantCulture));
                    break;
                case DateTimeOffset instant:
                    writer.WriteStringValue(instant.ToUniversalTime().ToString(Instant, CultureInfo.InvariantCulture));
                    break;
                case SelectedDimensions size:
                    writer.WriteStartObject();
                    writer.WritePropertyName("heightCm");
                    WriteValue(writer, size.HeightCm);
                    writer.WritePropertyName("lengthCm");
                    WriteValue(writer, size.LengthCm);
                    writer.WritePropertyName("widthCm");
                    WriteValue(writer, size.WidthCm);
                    writer.WriteEndObject();
                    break;
                case IEnumerable<string> codes:
                    writer.WriteStartArray();

                    foreach (var code in codes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                        writer.WriteStringValue(code);

                    writer.WriteEndArray();
                    break;
                case Enum code:
                    writer.WriteStringValue(code.ToString());
                    break;
                default:
                    throw new InvalidOperationException($"Selection field type {value.GetType().Name} cannot be written.");
            }
        }

        private static object? ReadValue(Type declared, JsonElement element)
        {
            var type = Nullable.GetUnderlyingType(declared) ?? declared;

            if (type == typeof(string))
                return element.ValueKind == JsonValueKind.String && element.GetString() is { Length: > 0 } text ? text : null;

            if (type == typeof(bool))
                return element.ValueKind == JsonValueKind.True ? true : null;

            if (type == typeof(int))
                return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var number) ? number : null;

            if (type == typeof(long))
                return element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var number) ? number : null;

            if (type == typeof(decimal))
                return element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var amount) ? amount : null;

            if (type == typeof(DateTimeOffset))
                return element.ValueKind == JsonValueKind.String && DateTimeOffset.TryParseExact(element.GetString(), Instant, CultureInfo.InvariantCulture, DateTimeStyles.None, out var instant) ? instant : null;

            if (type == typeof(SelectedDimensions))
                return element.ValueKind == JsonValueKind.Object
                       && element.TryGetProperty("heightCm", out var height) && height.TryGetDecimal(out var heightCm)
                       && element.TryGetProperty("lengthCm", out var length) && length.TryGetDecimal(out var lengthCm)
                       && element.TryGetProperty("widthCm", out var width) && width.TryGetDecimal(out var widthCm)
                    ? new SelectedDimensions(lengthCm, widthCm, heightCm)
                    : null;

            if (type == typeof(IReadOnlyList<string>))
                return element.ValueKind == JsonValueKind.Array && element.EnumerateArray().All(item => item.ValueKind == JsonValueKind.String)
                    ? element.EnumerateArray().Select(item => item.GetString()!).ToList()
                    : null;

            if (type.IsEnum)
                return element.ValueKind == JsonValueKind.String && Enum.TryParse(type, element.GetString(), false, out var code) && Enum.IsDefined(type, code) ? code : null;

            return null;
        }
    }
}
