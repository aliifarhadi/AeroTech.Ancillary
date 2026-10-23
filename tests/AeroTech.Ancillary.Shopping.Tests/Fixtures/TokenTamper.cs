using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class TokenTamper
{
    private const int PrefixLength = 2;

    public static string Claim<TValue>(string id, string claim, TValue value)
    {
        var parts = id[PrefixLength..].Split('.');
        var payload = JsonNode.Parse(Base64Url.DecodeFromChars(parts[1]))!.AsObject();

        payload[claim] = JsonSerializer.SerializeToNode(value);
        parts[1] = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload.ToJsonString()));

        return id[..PrefixLength] + string.Join('.', parts);
    }

    public static string Unsigned(string id)
    {
        var parts = id[PrefixLength..].Split('.');

        return id[..PrefixLength] + parts[0] + "." + parts[1] + ".";
    }
}
