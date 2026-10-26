using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedSelectionDto<TDetails>(
        string OfferItemId,
        string VariantCode,
        SelectionScope SelectionScope,
        OfferUnitDto Unit,
        OfferCoverageDto Coverage,
        ServiceSourceBindingDto SourceBinding,
        ServiceResolvedDefinitionDto Definition,
        ServiceResolvedProvisionDto Provision,
        ServiceResolvedPricingDto Pricing,
        TDetails Details,
        SelectedUnitDto Selected);
}
