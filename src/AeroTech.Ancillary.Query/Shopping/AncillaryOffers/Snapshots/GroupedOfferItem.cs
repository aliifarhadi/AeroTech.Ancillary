using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedOfferItem(
        AncillaryOfferItem Item,
        AncillaryServiceDefinition Definition,
        bool RequiresDetails,
        AncillarySelectionSnapshot? Evaluation,
        string? SelectionId)
    {
        public CanonicalAncillaryOfferCandidate Candidate => Evaluation?.Evaluation.Candidate ?? Item.Candidate;
    }
}
