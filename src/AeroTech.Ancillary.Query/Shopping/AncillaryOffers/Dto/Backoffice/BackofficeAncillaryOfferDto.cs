using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryOfferDto<TDetails>(
        string VariantCode,
        string Title,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Description,
        EnumValueDto SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        OfferMoneyDto UnitPrice,
        BackofficeQuantityPolicyDto QuantityPolicy,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SelectionId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<OfferQuantityOptionDto>? QuantityOptions,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Reference,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? OptionsEndpoint,
        TDetails Details);
}
