using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AncillaryOfferDto<TDetails>(
        string VariantCode,
        string Title,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Description,
        SelectionScope SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        OfferMoneyDto UnitPrice,
        OfferQuantityPolicyDto QuantityPolicy,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SelectionId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<OfferQuantityOptionDto>? QuantityOptions,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Reference,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? OptionsEndpoint,
        TDetails Details);
}
