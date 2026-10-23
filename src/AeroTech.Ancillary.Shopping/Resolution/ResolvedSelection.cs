using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Resolution
{
    public sealed record ResolvedSelection(
        string SelectionId,
        SelectionScope SelectionScope,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        string OfferBinding,
        string SalesBinding,
        AncillaryServiceDefinition Definition,
        AncillaryProvision Provision,
        AncillaryPricing Pricing,
        AncillaryPricingRate Rate,
        PricingUnit PricingUnit,
        int Quantity,
        decimal UnitAmount,
        decimal TotalAmount,
        ConfirmationRequirement ConfirmationRequirement,
        bool RequiresCheckAtReserve,
        string? SelectedJson);
}
