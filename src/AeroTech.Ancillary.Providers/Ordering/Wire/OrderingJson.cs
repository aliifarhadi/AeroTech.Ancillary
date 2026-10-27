using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Providers.Ordering.Wire
{
    public static class OrderingJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}
