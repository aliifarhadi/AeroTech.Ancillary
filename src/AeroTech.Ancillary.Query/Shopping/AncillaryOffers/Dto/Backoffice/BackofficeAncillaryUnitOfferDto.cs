using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryUnitOfferDto(
        EnumValueDto SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        OfferMoneyDto UnitPrice,
        BackofficeQuantityPolicyDto QuantityPolicy,
        string OfferItemId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SelectionOptionsDto<IReadOnlyDictionary<string, SelectionFieldDto>>? Options,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SelectedUnitDto? Selected);
}
