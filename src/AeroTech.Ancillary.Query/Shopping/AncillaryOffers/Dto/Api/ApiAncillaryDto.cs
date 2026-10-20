using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiAncillaryDto(
        string Type,
        ApiServiceDto Service,
        ApiPricingDto Pricing,
        ApiQuantityDto Quantity,
        IReadOnlyList<ApiSelectionRequirementDto> SelectionRequirements,
        ApiAvailabilityDto Availability,
        string ConfirmationRequirement,
        string Readiness,
        bool CanPurchase,
        IReadOnlyList<string> Reasons,
        string SelectionKind,
        string CoverageScope,
        IReadOnlyList<ApiOptionGroupDto> OptionGroups,
        OfferInteractionDto Interaction);
}
