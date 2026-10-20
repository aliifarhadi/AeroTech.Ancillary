using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
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

            Mark(conflicts, lines.Where(line => line.Ancillary is not null).GroupBy(line => line.Ancillary!.Item.OfferItemRef, StringComparer.Ordinal), ShoppingReasonCodes.DuplicateSelection);
            Mark(conflicts, seats.GroupBy(line => (line.Seat!.Flight.FlightRef, line.Seat.Offer.SeatNumber.ToUpperInvariant())), ShoppingReasonCodes.SeatSelectedTwice);
            Mark(conflicts, seats.GroupBy(line => (line.Seat!.Flight.FlightRef, line.Seat.Offer.TravellerRef)), ShoppingReasonCodes.TravellerSeatSelectedTwice);

            return lines
                .Select(line => conflicts.TryGetValue(line.ClientSelectionId, out var reasons)
                    ? line with { Status = SelectionEvaluationStatus.Rejected, CanPurchase = false, PricedSelection = null, ReasonCodes = [.. line.ReasonCodes.Union(reasons, StringComparer.Ordinal)] }
                    : line)
                .ToList();
        }

        public static SelectionBasket Basket(IReadOnlyList<SelectionLineSnapshot> lines, int currencyId, bool orderBound)
        {
            var sharedFees = lines
                .SelectMany(line => line.Ancillary?.Evaluation.Candidate.Price.UnappliedFeeLines ?? [])
                .Distinct()
                .ToList();
            var allReady = lines.All(line => line.CanPurchase) && sharedFees.Count == 0;
            var reasonCodes = new List<string>();

            if (sharedFees.Count > 0)
                reasonCodes.Add(ShoppingReasonCodes.SharedFeeNotApplied);

            if (!orderBound)
                reasonCodes.Add(ShoppingReasonCodes.OrderBindingRequired);

            return new SelectionBasket(allReady, currencyId, allReady ? lines.Sum(line => line.Total) : null, sharedFees, reasonCodes);
        }

        private static void Mark<TKey>(Dictionary<string, List<string>> conflicts, IEnumerable<IGrouping<TKey, SelectionLineSnapshot>> groups, string reasonCode)
        {
            foreach (var line in groups.Where(group => group.Skip(1).Any()).SelectMany(group => group))
            {
                if (!conflicts.TryGetValue(line.ClientSelectionId, out var reasons))
                    conflicts[line.ClientSelectionId] = reasons = [];

                reasons.Add(reasonCode);
            }
        }
    }
}
