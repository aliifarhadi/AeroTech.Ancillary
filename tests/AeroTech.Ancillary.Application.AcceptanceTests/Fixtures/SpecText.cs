using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class SpecText
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };

    private static readonly string[] EnumValueMembers = ["Name", "Title", "Value"];

    public static string Of(object? specification)
        => specification is null
            ? "null"
            : Canonical(JsonSerializer.SerializeToNode(specification, specification.GetType(), Options))?.ToJsonString() ?? "{}";

    public static IReadOnlyList<string> Members(object? specification)
        => specification is null
            ? []
            : JsonSerializer.SerializeToNode(specification, specification.GetType(), Options)!.AsObject()
                .Where(member => member.Value is not null)
                .Select(member => member.Key)
                .ToList();

    private static JsonNode? Canonical(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject value when value.Select(member => member.Key).OrderBy(key => key, StringComparer.Ordinal).SequenceEqual(EnumValueMembers):
                return JsonValue.Create(value["Name"]!.GetValue<string>());
            case JsonObject value:
                var members = value
                    .Select(member => (member.Key, Value: Canonical(member.Value)))
                    .Where(member => member.Value is not null)
                    .OrderBy(member => member.Key, StringComparer.Ordinal)
                    .ToList();

                return members.Count == 0 ? null : new JsonObject(members.Select(member => KeyValuePair.Create(member.Key, member.Value)));
            case JsonArray value:
                var items = value.Select(Canonical).Where(item => item is not null).OrderBy(item => item!.ToJsonString(), StringComparer.Ordinal).ToArray();

                return items.Length == 0 ? null : new JsonArray(items);
            case JsonValue value when value.GetValueKind() == JsonValueKind.Number:
                return JsonValue.Create(decimal.Parse(value.ToJsonString(), CultureInfo.InvariantCulture).ToString("G29", CultureInfo.InvariantCulture));
            case JsonValue value when value.GetValueKind() == JsonValueKind.String:
                return value.GetValue<string>().Length == 0 ? null : JsonValue.Create(value.GetValue<string>());
            case JsonValue value:
                return JsonValue.Create(value.GetValueKind() == JsonValueKind.True);
            default:
                return null;
        }
    }
}
