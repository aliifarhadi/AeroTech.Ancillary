using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Providers.FlightFlow.Wire
{
    public static class FlightFlowJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}
