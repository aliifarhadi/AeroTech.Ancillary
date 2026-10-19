using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiIncludedDto(
        IReadOnlyDictionary<string, ApiBagDto> Bags,
        [property: JsonPropertyName("other-services")] IReadOnlyDictionary<string, ApiOtherServiceDto> OtherServices);
}
