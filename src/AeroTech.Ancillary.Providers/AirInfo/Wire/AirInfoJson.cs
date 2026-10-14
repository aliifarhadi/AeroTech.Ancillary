using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Providers.AirInfo.Wire
{
    public static class AirInfoJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}
