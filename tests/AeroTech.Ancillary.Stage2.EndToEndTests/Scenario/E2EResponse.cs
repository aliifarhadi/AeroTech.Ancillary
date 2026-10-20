using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed record E2EResponse(int Status, JsonNode? Body)
{
    public JsonNode Data => Body?["data"] ?? throw new InvalidOperationException($"HTTP {Status} has no data: {Body?.ToJsonString()}");

    public int? ErrorCode => Body?["errors"]?[0]?["code"]?.GetValue<int>();

    public string? Error => Body?["error"]?.GetValue<string>();
}
