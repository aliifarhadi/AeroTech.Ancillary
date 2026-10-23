using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedDefinitionDto(
        long DefinitionVersionId,
        string ServiceDefinitionRef,
        string VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Rfic,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Rfisc,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SsrCode);
}
