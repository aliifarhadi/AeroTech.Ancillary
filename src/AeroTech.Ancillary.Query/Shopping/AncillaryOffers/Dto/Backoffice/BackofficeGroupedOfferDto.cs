using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public abstract record BackofficeGroupedOfferDto(
        [property: JsonPropertyOrder(-1)] string OfferItemRef,
        [property: JsonPropertyOrder(-1)] string VariantCode,
        [property: JsonPropertyOrder(-1)] string ServiceDefinitionRef,
        [property: JsonPropertyOrder(-1)] string Name,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> TravellerRefs,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> FlightRefs,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> PortionRefs,
        [property: JsonPropertyOrder(-1)] EnumValueDto CoverageScope,
        [property: JsonPropertyOrder(-1)] EnumValueDto Readiness,
        [property: JsonPropertyOrder(-1)] BackofficeGroupedPriceDto Price,
        [property: JsonPropertyOrder(-1)] BackofficeGroupedQuantityDto? Quantity,
        [property: JsonPropertyOrder(-1)] string? SelectionId,
        [property: JsonPropertyOrder(-1)] bool RequiresDetails,
        [property: JsonPropertyOrder(-1)] string? DetailsEndpoint);
}
