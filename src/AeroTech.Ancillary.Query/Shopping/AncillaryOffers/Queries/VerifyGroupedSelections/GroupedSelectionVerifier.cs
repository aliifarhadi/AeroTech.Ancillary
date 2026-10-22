using System.Globalization;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyGroupedSelections
{
    public sealed class GroupedSelectionVerifier : IGroupedSelectionVerifier
    {
        private readonly IOrderCommercialContextReader _orders;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IEvaluateBatchService _batches;

        public GroupedSelectionVerifier(IOrderCommercialContextReader orders, IOperatorAirlineResolver operatorAirline, IEvaluateBatchService batches)
        {
            _orders = orders;
            _operatorAirline = operatorAirline;
            _batches = batches;
        }

        public async Task<IReadOnlyList<VerifiedGroupedSelection>> VerifyAsync(GroupedSelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var order = await _orders.FindAsync(verification.OrderId, cancellationToken) ?? throw Invalid(nameof(verification.OrderId));

            Require(order.CommercialVersion == verification.OrderCommercialVersion, nameof(verification.OrderCommercialVersion));
            Require(order.AcceptsChanges, nameof(order.AcceptsChanges));
            Require(order.Air.CurrencyId == verification.CurrencyId, nameof(verification.CurrencyId));
            Require(order.TicketedAtUtc?.ToUniversalTime() == verification.TicketedAtUtc?.ToUniversalTime(), nameof(verification.TicketedAtUtc));

            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var bound = verification.Selections.Select(entry => Bind(entry, order, verification)).ToList();
            var verified = new VerifiedGroupedSelection?[bound.Count];

            foreach (var group in Enumerable.Range(0, bound.Count).GroupBy(index => (bound[index].Id.SourceKind, bound[index].Id.Scope)))
            {
                var first = bound[group.First()].Id;
                var query = first.SourceKind == ShoppingSourceKind.Offer
                    ? new GroupedSelectionQuery(first.SourceRef, null)
                    : new GroupedSelectionQuery(null, verification.OrderId);
                var batch = await _batches.ExecuteAsync(
                    query,
                    group.Select(index => new SelectionLineRequest(index.ToString(CultureInfo.InvariantCulture), bound[index].Id.OfferItemRef, bound[index].Id.Selection)).ToList(),
                    first.Scope.ToSalesScope((int)homeAirlineId.Value),
                    cancellationToken);
                var context = batch.Shopping.PointsOfSale.First().Context;

                foreach (var line in batch.Lines)
                {
                    var index = int.Parse(line.ClientSelectionId, CultureInfo.InvariantCulture);

                    verified[index] = Verified(bound[index], line, batch.Shopping, context, order);
                }
            }

            return verified.Select(selection => selection!).ToList();
        }

        private static Bound Bind(GroupedHoldSelectionEntry entry, OrderCommercialContext order, GroupedSelectionVerification verification)
        {
            var field = $"{entry.Profile}[{entry.OrderServiceId}].{nameof(entry.SelectionId)}";
            var id = NonSeatSelectionIdCodec.Decode(entry.SelectionId) ?? throw Invalid($"{field}:{ShoppingReasonCodes.PricedSelectionNotAuthentic}");

            Require(id.Profile == entry.Profile, $"{field}:{ShoppingReasonCodes.OfferItemNotOffered}");
            Require(id.Scope.PointOfSaleId == verification.PointOfSaleId, $"{field}:{ShoppingReasonCodes.PointOfSaleNotAllowed}");
            Require(
                id.SourceKind == ShoppingSourceKind.Offer
                    ? string.Equals(id.SourceRef, order.Air.OfferId, StringComparison.Ordinal)
                    : id.SourceRef == order.OrderId.ToString(CultureInfo.InvariantCulture) && id.OrderCommercialVersion == order.CommercialVersion,
                $"{field}:{ShoppingReasonCodes.PricedSelectionForAnotherOrder}");

            var service = order.Services.FirstOrDefault(service => service.OrderServiceId == entry.OrderServiceId) ?? throw Invalid($"{entry.Profile}[{entry.OrderServiceId}].{nameof(entry.OrderServiceId)}");

            Require(string.Equals(service.AcceptedSelectionId, entry.SelectionId, StringComparison.Ordinal), $"{field}:AcceptedSelectionId");
            Require(service.AcceptedCurrencyId == verification.CurrencyId && service.AcceptedTotal is not null, $"{field}:AcceptedTotal");

            return new Bound(entry, id, service);
        }

        private static VerifiedGroupedSelection Verified(Bound bound, SelectionLineSnapshot line, AncillaryShoppingSnapshot shopping, AncillaryShoppingContext context, OrderCommercialContext order)
        {
            var field = $"{bound.Entry.Profile}[{bound.Entry.OrderServiceId}].{nameof(bound.Entry.SelectionId)}";

            Require(line.Status != SelectionEvaluationStatus.Rejected && line.CanPurchase, $"{field}:{string.Join(',', line.ReasonCodes.Union(line.Candidate?.ReasonCodes ?? [], StringComparer.Ordinal))}");

            var ancillary = line.Ancillary!;
            var candidate = ancillary.Evaluation.Candidate;

            Require(string.Equals(ancillary.CurrentOfferItemRef, bound.Id.OfferItemRef, StringComparison.Ordinal), $"{field}:{ShoppingReasonCodes.SourceVersionChanged}");
            Require(
                string.Equals(GroupedSelectionIdProvider.IdOf(ancillary, bound.Id.SourceKind == ShoppingSourceKind.Order ? order.OrderId : null), bound.Entry.SelectionId, StringComparison.Ordinal),
                $"{field}:{ShoppingReasonCodes.SignedFactsChanged}");
            Require(line.CurrencyId == shopping.CurrencyId && line.Total == bound.Service.AcceptedTotal, $"{field}:{ShoppingReasonCodes.PriceChanged}");
            Require(candidate.Price.UnappliedFeeLines.Count == 0, $"{field}:{ShoppingReasonCodes.SharedFeeNotApplied}");
            Require(
                candidate.Availability is { InventoryAuthority: InventoryAuthority.Unlimited, RequiresCheckAtReserve: false },
                $"{field}:{(candidate.Availability.InventoryAuthority is null ? ShoppingReasonCodes.InventoryNotConfigured : ShoppingReasonCodes.AvailabilityMustBeChecked)}");

            var travellers = order.Travellers.Where(traveller => candidate.Travellers.Contains(traveller.TravellerRef, StringComparer.Ordinal)).ToList();
            long? travellerId = candidate.Scope.CoverageScope == ServiceCoverageScope.Order ? null : travellers is [{ OrderTravellerId: > 0 } only] ? only.OrderTravellerId : null;

            Require(candidate.Scope.CoverageScope == ServiceCoverageScope.Order || travellerId is not null, $"{field}:{ShoppingReasonCodes.OrderTravellerNotLinked}");

            return new VerifiedGroupedSelection(
                bound.Entry.OrderServiceId,
                bound.Entry.SelectionId,
                bound.Entry.Profile,
                candidate.DefinitionVersionId,
                candidate.ProvisionId,
                travellerId,
                candidate.Scope.CoverageScope,
                context.Flights.Where(flight => candidate.FlightRefs.Contains(flight.FlightRef, StringComparer.Ordinal)).Select(flight => flight.FlightId).Distinct().ToList(),
                ancillary.Selection.Quantity ?? 1);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw Invalid(field);
        }

        private static Exception Invalid(string field) => ExceptionFactory.AncillaryHoldRequestIsInvalid(field);

        private sealed record Bound(GroupedHoldSelectionEntry Entry, NonSeatSelectionId Id, OrderServiceFacts Service);
    }
}
