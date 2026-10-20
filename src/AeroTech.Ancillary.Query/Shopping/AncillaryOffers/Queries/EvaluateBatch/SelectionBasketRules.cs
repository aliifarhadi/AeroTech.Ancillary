using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public static class SelectionBasketRules
    {
        public static IReadOnlyList<SelectionLineSnapshot> Settle(IReadOnlyList<SelectionLineSnapshot> lines)
        {
            var conflicts = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var seats = lines.Where(line => line.Seat is not null).ToList();
            var repeated = lines
                .Where(line => line.Ancillary is not null)
                .GroupBy(line => line.Ancillary!.Item.OfferItemRef, StringComparer.Ordinal)
                .Where(group => group.Skip(1).Any());

            foreach (var group in repeated)
            {
                foreach (var line in group)
                    Add(conflicts, line, Repetition(line, group.Where(other => !ReferenceEquals(other, line)).ToList()));
            }

            Mark(conflicts, seats.GroupBy(line => (line.Seat!.Flight.FlightRef, line.Seat.Offer.SeatNumber.ToUpperInvariant())), ShoppingReasonCodes.SeatSelectedTwice);
            Mark(conflicts, seats.GroupBy(line => (line.Seat!.Flight.FlightRef, line.Seat.Offer.TravellerRef)), ShoppingReasonCodes.TravellerSeatSelectedTwice);

            return lines
                .Select(line => conflicts.TryGetValue(line.ClientSelectionId, out var reasons)
                    ? line with { Status = SelectionEvaluationStatus.Rejected, CanPurchase = false, PricedSelection = null, ReasonCodes = [.. line.ReasonCodes.Union(reasons, StringComparer.Ordinal)] }
                    : line)
                .ToList();
        }

        public static IReadOnlyList<SelectionLineSnapshot> Link(IReadOnlyList<SelectionLineSnapshot> lines, AncillaryShoppingContext context)
        {
            var linked = context.Travellers.Where(traveller => traveller.OrderTravellerId is > 0).Select(traveller => traveller.TravellerRef).ToHashSet(StringComparer.Ordinal);

            return lines
                .Select(line => Beneficiaries(line).All(linked.Contains)
                    ? line
                    : line with
                    {
                        Status = line.Status == SelectionEvaluationStatus.Rejected ? SelectionEvaluationStatus.Rejected : SelectionEvaluationStatus.Incomplete,
                        CanPurchase = false,
                        PricedSelection = null,
                        ReasonCodes = [.. line.ReasonCodes.Union([ShoppingReasonCodes.OrderTravellerNotLinked], StringComparer.Ordinal)]
                    })
                .ToList();
        }

        public static SelectionBasket Basket(IReadOnlyList<SelectionLineSnapshot> lines, int currencyId, bool orderBound)
        {
            var sharedFees = lines
                .SelectMany(line => line.Ancillary?.Evaluation.Candidate.Price.UnappliedFeeLines ?? [])
                .Distinct()
                .ToList();
            var priced = lines.All(line => line.CanPurchase && line.CurrencyId == currencyId) && sharedFees.Count == 0;
            var reasonCodes = new List<string>();

            if (sharedFees.Count > 0)
                reasonCodes.Add(ShoppingReasonCodes.SharedFeeNotApplied);

            if (!orderBound)
                reasonCodes.Add(ShoppingReasonCodes.OrderBindingRequired);

            return new SelectionBasket(priced && orderBound, currencyId, priced ? lines.Sum(line => line.Total) : null, sharedFees, reasonCodes);
        }

        private static IEnumerable<string> Beneficiaries(SelectionLineSnapshot line)
            => line.Seat is { } seat ? [seat.Offer.TravellerRef] : line.Ancillary?.Evaluation.Candidate.Travellers ?? [];

        private static string Repetition(SelectionLineSnapshot line, IReadOnlyList<SelectionLineSnapshot> others)
        {
            var selection = line.Ancillary!.Selection;

            if (others.Any(other => SelectionCanonicalForm.Of(other.Ancillary!.Selection) == SelectionCanonicalForm.Of(selection)))
                return ShoppingReasonCodes.DuplicateSelection;

            return others.Any(other => SelectionCanonicalForm.Of(other.Ancillary!.Selection with { Quantity = null }) == SelectionCanonicalForm.Of(selection with { Quantity = null }))
                ? ShoppingReasonCodes.QuantitySplitAcrossLines
                : ShoppingReasonCodes.RepeatedInstanceNotAllowed;
        }

        private static void Mark<TKey>(Dictionary<string, List<string>> conflicts, IEnumerable<IGrouping<TKey, SelectionLineSnapshot>> groups, string reasonCode)
        {
            foreach (var line in groups.Where(group => group.Skip(1).Any()).SelectMany(group => group))
                Add(conflicts, line, reasonCode);
        }

        private static void Add(Dictionary<string, List<string>> conflicts, SelectionLineSnapshot line, string reasonCode)
        {
            if (!conflicts.TryGetValue(line.ClientSelectionId, out var reasons))
                conflicts[line.ClientSelectionId] = reasons = [];

            reasons.Add(reasonCode);
        }
    }
}
