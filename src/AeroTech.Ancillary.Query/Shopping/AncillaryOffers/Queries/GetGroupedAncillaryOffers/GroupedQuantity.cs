using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public static class GroupedQuantity
    {
        private const string Quantity = "Quantity";

        public static QuantityAssessment? Of(CanonicalAncillaryOfferCandidate candidate)
            => candidate.Selection.Fields.Any(field => string.Equals(field.Name, Quantity, StringComparison.Ordinal)) ? candidate.Quantity : null;
    }
}
