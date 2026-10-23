using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedSelectionDto<TDetails>(
        string SelectionId,
        string VariantCode,
        SelectionScope SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        ServiceSourceBindingDto SourceBinding,
        ServiceResolvedDefinitionDto Definition,
        ServiceResolvedProvisionDto Provision,
        ServiceResolvedPricingDto Pricing,
        TDetails Details,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonElement? Selected);
}
