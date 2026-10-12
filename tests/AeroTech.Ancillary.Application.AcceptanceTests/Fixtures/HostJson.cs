using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AeroTech.Framework.Presentation.Json;
using AeroTech.Framework.Presentation.Responses;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class HostJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(), new LongToStringJsonConverter(), new NullableLongToStringJsonConverter() }
    };

    public static readonly JsonSerializerOptions Indented = new(Options) { WriteIndented = true };

    public static TRequest Bind<TRequest>(string body)
        => JsonSerializer.Deserialize<TRequest>(body, Options) ?? throw new JsonException("The request body is empty.");

    public static JsonNode Data(IActionResult result)
    {
        var value = Assert.IsType<OkObjectResult>(result).Value!;
        var envelope = Activator.CreateInstance(typeof(ApiResult<>).MakeGenericType(value.GetType()), value)!;

        return JsonNode.Parse(JsonSerializer.Serialize(envelope, envelope.GetType(), Options))!["data"]!;
    }

    public static string Text(JsonNode node) => node.ToJsonString(Indented).ReplaceLineEndings("\n");
}
