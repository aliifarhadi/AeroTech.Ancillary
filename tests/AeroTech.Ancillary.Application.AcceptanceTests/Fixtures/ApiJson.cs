using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Framework.Presentation.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = ApiOptions();

    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;

    public static void Malformed<T>(string json) => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<T>(json, Options));

    public static string With(string json, string path, JsonNode? value)
    {
        var root = JsonNode.Parse(json)!;
        var segments = path.Split('.');
        var target = segments[..^1].Aggregate(root, (node, segment) => node[segment]!);

        target[segments[^1]] = value;

        return root.ToJsonString();
    }

    public static void AssertSame(JsonNode? expected, JsonNode? actual, string path = "$")
    {
        switch (expected)
        {
            case null:
                Assert.True(actual is null, $"{path}: expected null.");
                break;
            case JsonObject expectedObject:
                var actualObject = Assert.IsType<JsonObject>(actual);

                Assert.Equal(
                    expectedObject.Select(member => member.Key).Order(StringComparer.Ordinal),
                    actualObject.Select(member => member.Key).Order(StringComparer.Ordinal));

                foreach (var (name, value) in expectedObject)
                    AssertSame(value, actualObject[name], $"{path}.{name}");

                break;
            case JsonArray expectedArray:
                var actualArray = Assert.IsType<JsonArray>(actual);

                Assert.True(expectedArray.Count == actualArray.Count, $"{path}: expected {expectedArray.Count} entries, found {actualArray.Count}.");

                for (var index = 0; index < expectedArray.Count; index++)
                    AssertSame(expectedArray[index], actualArray[index], $"{path}[{index}]");

                break;
            default:
                Assert.True(actual is JsonValue, $"{path}: expected a value.");

                var expectedValue = expected.GetValue<JsonElement>();
                var actualValue = actual!.GetValue<JsonElement>();

                Assert.True(expectedValue.ValueKind == actualValue.ValueKind, $"{path}: expected {expectedValue.ValueKind}, found {actualValue.ValueKind}.");

                if (expectedValue.ValueKind == JsonValueKind.Number)
                    Assert.True(expectedValue.GetDecimal() == actualValue.GetDecimal(), $"{path}: expected {expectedValue}, found {actualValue}.");
                else
                    Assert.True(expectedValue.ToString() == actualValue.ToString(), $"{path}: expected {expectedValue}, found {actualValue}.");

                break;
        }
    }

    private static JsonSerializerOptions ApiOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Authority)}"] = "https://identity.test/",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = "pss-api"
            })
            .Build();

        using var services = new ServiceCollection().AddPresentation(configuration).BuildServiceProvider();

        return services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
    }
}
