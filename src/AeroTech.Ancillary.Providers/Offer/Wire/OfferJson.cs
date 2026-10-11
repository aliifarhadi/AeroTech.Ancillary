using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public static class OfferJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
