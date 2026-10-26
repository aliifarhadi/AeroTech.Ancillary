using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AncillaryUnitOfferDto(
        SelectionScope SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        OfferMoneyDto UnitPrice,
        OfferQuantityPolicyDto QuantityPolicy,
        string OfferItemId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SelectionOptionsDto<IReadOnlyDictionary<string, SelectionFieldDto>>? Options,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SelectedUnitDto? Selected);
}
