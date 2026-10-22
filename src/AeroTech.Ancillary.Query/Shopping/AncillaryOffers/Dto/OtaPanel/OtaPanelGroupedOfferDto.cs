using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public abstract record OtaPanelGroupedOfferDto(
        [property: JsonPropertyOrder(-1)] string OfferItemRef,
        [property: JsonPropertyOrder(-1)] string VariantCode,
        [property: JsonPropertyOrder(-1)] string ServiceDefinitionRef,
        [property: JsonPropertyOrder(-1)] string Name,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> TravellerRefs,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> FlightRefs,
        [property: JsonPropertyOrder(-1)] IReadOnlyList<string> PortionRefs,
        [property: JsonPropertyOrder(-1)] string CoverageScope,
        [property: JsonPropertyOrder(-1)] string Readiness,
        [property: JsonPropertyOrder(-1)] OtaPanelGroupedPriceDto Price,
        [property: JsonPropertyOrder(-1)] string? SelectionId,
        [property: JsonPropertyOrder(-1)] bool RequiresDetails,
        [property: JsonPropertyOrder(-1)] string? DetailsEndpoint);
}
