using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using AeroTech.Ancillary.Shopping.Options;
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
        string? OfferItemId = null,
        IReadOnlyList<SelectionFieldDescriptor>? Options = null,
        SelectionSnapshotLine? Selected = null);
}
