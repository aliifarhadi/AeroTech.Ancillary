using System.Globalization;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyGroupedSelections
{
    public sealed class GroupedSelectionVerifier : IGroupedSelectionVerifier
    {
        private readonly IOrderCommercialContextReader _orders;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IShoppingSource _source;
        private readonly IAncillaryShoppingEngine _engine;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public GroupedSelectionVerifier(
            IOrderCommercialContextReader orders,
            IOperatorAirlineResolver operatorAirline,
            IShoppingSource source,
            IAncillaryShoppingEngine engine,
            IActiveAncillaryDefinitionReader definitions)
        {
            _orders = orders;
            _operatorAirline = operatorAirline;
            _source = source;
            _engine = engine;
            _definitions = definitions;
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

            var definitions = (await _definitions.ListActiveAsync((int)homeAirlineId.Value, cancellationToken)).ToDictionary(definition => definition.Id);
            var bound = verification.Selections.Select(entry => Bind(entry, order, verification, definitions)).ToList();
            var held = bound.Select(row => row.Entry.OrderServiceId).ToHashSet();
            var verified = new VerifiedGroupedSelection?[bound.Count];

            foreach (var group in Enumerable.Range(0, bound.Count).GroupBy(index => bound[index].Id.Scope))
            {
                var scope = group.Key.ToSalesScope((int)homeAirlineId.Value);
                var shopping = await _source.LoadAsync(null, verification.OrderId, scope, cancellationToken);
                var context = shopping.PointsOfSale.Single(pointOfSale => pointOfSale.PointOfSaleId == group.Key.PointOfSaleId).Context;

                context = context with { ExistingServiceFacts = context.ExistingServiceFacts.Where(fact => fact.OrderServiceId is not { } id || !held.Contains(id)).ToList() };

                var offered = await _engine.ShopAsync(
                    context,
                    new ShoppingFilter { VariantCodes = group.Select(index => bound[index].Id.VariantCode).Distinct(StringComparer.Ordinal).ToList() },
                    cancellationToken);

                foreach (var index in group)
                    verified[index] = await VerifiedAsync(bound[index], order, context, offered, verification, cancellationToken);
            }

            return verified.Select(selection => selection!).ToList();
        }

        private static Bound Bind(
            GroupedHoldSelectionEntry entry,
            OrderCommercialContext order,
            GroupedSelectionVerification verification,
            IReadOnlyDictionary<long, AncillaryServiceDefinition> definitions)
        {
            var field = $"{entry.Profile}[{entry.OrderServiceId}].{nameof(entry.SelectionId)}";
            var id = NonSeatSelectionIdCodec.Decode(entry.SelectionId) ?? throw Invalid($"{field}:{ShoppingReasonCodes.PricedSelectionNotAuthentic}");

            Require(id.Profile == entry.Profile && id.Profile != AncillaryProfile.Seat, $"{field}:{ShoppingReasonCodes.OfferItemNotOffered}");
            Require(id.Scope.PointOfSaleId == verification.PointOfSaleId, $"{field}:{ShoppingReasonCodes.PointOfSaleNotAllowed}");
            Require(
                id.SourceKind == ShoppingSourceKind.Offer
                    ? string.Equals(id.SourceRef, order.Air.OfferId, StringComparison.Ordinal)
                    : id.SourceRef == order.OrderId.ToString(CultureInfo.InvariantCulture) && id.OrderCommercialVersion <= order.CommercialVersion,
                $"{field}:{ShoppingReasonCodes.PricedSelectionForAnotherOrder}");

            var service = order.Services.FirstOrDefault(service => service.OrderServiceId == entry.OrderServiceId) ?? throw Invalid($"{entry.Profile}[{entry.OrderServiceId}].{nameof(entry.OrderServiceId)}");

            Require(string.Equals(service.AcceptedSelectionId, entry.SelectionId, StringComparison.Ordinal), $"{field}:AcceptedSelectionId");
            Require(service.AcceptedCurrencyId == verification.CurrencyId && service.AcceptedTotal is not null, $"{field}:AcceptedTotal");
            Require(service is { AcceptedServiceDefinitionId: > 0, AcceptedProvisionId: > 0 }, $"{field}:AcceptedServiceDefinitionId");
            Require(service.CommercialState == ExistingServiceCommercialState.Active, $"{field}:{nameof(service.CommercialState)}");
            Require(definitions.TryGetValue(service.AcceptedServiceDefinitionId!.Value, out var definition) && definition.VariantCode == id.VariantCode, $"{field}:{ShoppingReasonCodes.OfferItemNotOffered}");

            return new Bound(entry, id, service);
        }

        private async Task<VerifiedGroupedSelection> VerifiedAsync(
            Bound bound,
            OrderCommercialContext order,
            AncillaryShoppingContext context,
            CanonicalAncillaryOfferResult offered,
            GroupedSelectionVerification verification,
            CancellationToken cancellationToken)
        {
            var field = $"{bound.Entry.Profile}[{bound.Entry.OrderServiceId}].{nameof(bound.Entry.SelectionId)}";
            var service = bound.Service;
            var expectedTravellers = service.TravellerRef.Length == 0 ? [] : new[] { service.TravellerRef };
            var current = offered.Candidates.FirstOrDefault(candidate => candidate.DefinitionVersionId == service.AcceptedServiceDefinitionId
                                                                       && candidate.ProvisionId == service.AcceptedProvisionId
                                                                       && candidate.Travellers.SequenceEqual(expectedTravellers, StringComparer.Ordinal)
                                                                       && candidate.FlightRefs.Order(StringComparer.Ordinal).SequenceEqual(service.FlightRefs.Order(StringComparer.Ordinal), StringComparer.Ordinal))
                          ?? throw Invalid($"{field}:{ShoppingReasonCodes.OfferItemNotOffered}");

            Require(service.PortionRef is null || current.PortionRefs.Contains(service.PortionRef, StringComparer.Ordinal), $"{field}:{nameof(service.PortionRef)}");

            var evaluation = await _engine.EvaluateSelectionAsync(context, current.CandidateIdentity, bound.Id.Selection, cancellationToken);
            var candidate = evaluation.Candidate;

            Require(
                evaluation is { Status: SelectionEvaluationStatus.Accepted, ProvisionChangedBySelection: false } && OfferPurchase.CanPurchase(evaluation),
                $"{field}:{string.Join(',', evaluation.FieldIssues.Select(issue => issue.ReasonCode).Union(candidate.ReasonCodes, StringComparer.Ordinal))}");
            Require(candidate.Price.PricingRevisionId == service.AcceptedPricingRevisionId, $"{field}:{ShoppingReasonCodes.PriceChanged}");
            Require(candidate.Price.CurrencyId == verification.CurrencyId && candidate.Price.RequestedQuantityTotal == service.AcceptedTotal, $"{field}:{ShoppingReasonCodes.PriceChanged}");
            Require(candidate.Price.Units == service.Quantity, $"{field}:{nameof(service.Quantity)}");
            Require(candidate.Price.UnappliedFeeLines.Count == 0, $"{field}:{ShoppingReasonCodes.SharedFeeNotApplied}");
            Require(GroupedHoldCapability.Blocker(candidate) is null, $"{field}:{GroupedHoldCapability.Blocker(candidate)}");
            Require(DirectSelection.HasConsent(candidate, bound.Id.Selection), $"{field}:{ShoppingReasonCodes.SelectionFieldRequired}");

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
                service.Quantity);
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
