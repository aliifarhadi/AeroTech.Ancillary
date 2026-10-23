using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public sealed record AncillaryOfferRow(
        CanonicalAncillaryOfferCandidate Candidate,
        AncillaryServiceDefinition Definition,
        AncillaryProvision Provision,
        SelectionScope SelectionScope,
        decimal UnitAmount,
        AncillaryQuantityPolicy QuantityPolicy,
        string? SelectionId = null,
        IReadOnlyList<AncillaryQuantityOption>? QuantityOptions = null,
        string? Reference = null);
}
