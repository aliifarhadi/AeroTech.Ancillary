using System.Globalization;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections
{
    public sealed class ResolveSelectionsService : IResolveSelectionsService
    {
        private static readonly string[] SourceChanges = [ShoppingReasonCodes.OfferItemNotOffered];

        private readonly IPricedSelectionTokens _tokens;
        private readonly IEvaluateBatchService _batches;

        public ResolveSelectionsService(IPricedSelectionTokens tokens, IEvaluateBatchService batches)
        {
            _tokens = tokens;
            _batches = batches;
        }

        public async Task<SelectionResolutionSnapshot> ExecuteAsync(
            long? orderId,
            int? expectedOrderCommercialVersion,
            IReadOnlyList<string>? pricedSelectionRefs,
            CancellationToken cancellationToken = default)
        {
            if (orderId is not { } order || order <= 0)
                throw ExceptionFactory.ShoppingContextIsInvalid("an orderId is required");

            if (expectedOrderCommercialVersion is not { } expectedVersion || expectedVersion <= 0)
                throw ExceptionFactory.ShoppingContextIsInvalid("an expectedOrderCommercialVersion is required");

            if (pricedSelectionRefs is not { Count: > 0 and <= EvaluateBatchService.MaximumLines })
                throw ExceptionFactory.ShoppingContextIsInvalid($"pricedSelectionRefs must hold between 1 and {EvaluateBatchService.MaximumLines} references");

            var readings = new List<PricedSelectionReading>(pricedSelectionRefs.Count);

            foreach (var reference in pricedSelectionRefs)
                readings.Add(await _tokens.ReadAsync(reference));

            var results = new ResolvedSelectionSnapshot?[pricedSelectionRefs.Count];

            for (var index = 0; index < readings.Count; index++)
                results[index] = readings[index] switch
                {
                    { Status: SelectionResolutionStatus.Rejected } => Unresolved(pricedSelectionRefs[index], SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionNotAuthentic),
                    { Status: SelectionResolutionStatus.Expired } => Unresolved(pricedSelectionRefs[index], SelectionResolutionStatus.Expired),
                    { Selection.OrderId: var bound } when bound != order => Unresolved(pricedSelectionRefs[index], SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionForAnotherOrder),
                    _ => null
                };

            int? currentVersion = null;
            var evaluated = new List<(int Index, SelectionLineSnapshot Line, AncillaryShoppingContext Context)>();

            foreach (var group in Enumerable.Range(0, readings.Count).Where(index => results[index] is null).GroupBy(index => ScopeKey(readings[index].Selection!.Scope)))
            {
                var batch = await _batches.ExecuteAsync(
                    new OrderShoppingQuery(order),
                    group.Select(index => Request(index, readings[index].Selection!)).ToList(),
                    readings[group.First()].Selection!.Scope,
                    cancellationToken);
                var context = batch.Shopping.PointsOfSale.First().Context;

                currentVersion = context.SourceIdentity.OrderCommercialVersion;
                evaluated.AddRange(batch.Lines.Select(line => (int.Parse(line.ClientSelectionId, CultureInfo.InvariantCulture), line, context)));
            }

            var settled = SelectionBasketRules.Settle(evaluated.Select(row => row.Line).ToList());

            for (var position = 0; position < evaluated.Count; position++)
            {
                var (index, _, context) = evaluated[position];

                results[index] = Resolve(pricedSelectionRefs[index], readings[index].Selection!, settled[position], context, expectedVersion);
            }

            return new SelectionResolutionSnapshot(order, currentVersion, results.Select(result => result!).ToList());
        }

        private static ResolvedSelectionSnapshot Resolve(string reference, PricedSelection token, SelectionLineSnapshot line, AncillaryShoppingContext context, int expectedVersion)
        {
            var currentVersion = context.SourceIdentity.OrderCommercialVersion;

            if (currentVersion != expectedVersion || currentVersion != token.OrderCommercialVersion)
                return Unresolved(reference, SelectionResolutionStatus.Stale, ShoppingReasonCodes.OrderVersionChanged);

            if (OrderBinding.Of(context.SourceIdentity) is not { } binding || !string.Equals(binding.SourceVersion, token.SourceVersion, StringComparison.Ordinal))
                return Unresolved(reference, SelectionResolutionStatus.Stale, ShoppingReasonCodes.SourceVersionChanged);

            if (line.Status == SelectionEvaluationStatus.Rejected)
                return new ResolvedSelectionSnapshot(
                    reference,
                    line.ReasonCodes.Any(reason => SourceChanges.Contains(reason, StringComparer.Ordinal)) ? SelectionResolutionStatus.Stale : SelectionResolutionStatus.Rejected,
                    RejectionReasons(line),
                    null,
                    null);

            if (!line.CanPurchase)
                return new ResolvedSelectionSnapshot(
                    reference,
                    line.ReasonCodes.Contains(ShoppingReasonCodes.OrderTravellerNotLinked, StringComparer.Ordinal)
                        ? SelectionResolutionStatus.NeedsVerification
                        : line.Candidate?.OfferReadiness switch
                        {
                            OfferReadiness.NeedsQuote => SelectionResolutionStatus.NeedsQuote,
                            OfferReadiness.NeedsVerification => SelectionResolutionStatus.NeedsVerification,
                            _ => SelectionResolutionStatus.Unavailable
                        },
                    [.. line.ReasonCodes.Union(line.Candidate?.ReasonCodes ?? [], StringComparer.Ordinal)],
                    null,
                    null);

            var fresh = PricedSelectionMapper.Of(line, token.Scope, binding);

            if (!SameFacts(token, fresh))
                return Unresolved(reference, SelectionResolutionStatus.Stale, ShoppingReasonCodes.SignedFactsChanged);

            return fresh.CurrencyId == token.CurrencyId && fresh.Total == token.Total
                ? new ResolvedSelectionSnapshot(reference, SelectionResolutionStatus.Valid, [], line, context)
                : new ResolvedSelectionSnapshot(reference, SelectionResolutionStatus.Stale, [ShoppingReasonCodes.PriceChanged], line, context);
        }

        private static bool SameFacts(PricedSelection signed, PricedSelection fresh)
            => signed.OfferItemRef == fresh.OfferItemRef
               && signed.CoverageScope == fresh.CoverageScope
               && signed.TravellerRefs.SequenceEqual(fresh.TravellerRefs, StringComparer.Ordinal)
               && signed.FlightRefs.SequenceEqual(fresh.FlightRefs, StringComparer.Ordinal)
               && signed.PortionRefs.SequenceEqual(fresh.PortionRefs, StringComparer.Ordinal)
               && signed.ServiceDefinitionRef == fresh.ServiceDefinitionRef
               && signed.DefinitionVersionId == fresh.DefinitionVersionId
               && signed.ProvisionId == fresh.ProvisionId
               && signed.PricingRevisionId == fresh.PricingRevisionId
               && signed.Units == fresh.Units
               && SelectionCanonicalForm.Of(signed.Selection ?? new AncillarySelection()) == SelectionCanonicalForm.Of(fresh.Selection ?? new AncillarySelection());

        private static IReadOnlyList<string> RejectionReasons(SelectionLineSnapshot line)
            => [.. line.ReasonCodes.Union(line.Ancillary?.Evaluation.FieldIssues.Select(issue => issue.ReasonCode) ?? [], StringComparer.Ordinal)];

        private static SelectionLineRequest Request(int index, PricedSelection token)
            => new(index.ToString(CultureInfo.InvariantCulture), token.OfferItemRef, token.Selection ?? new AncillarySelection());

        private static ResolvedSelectionSnapshot Unresolved(string reference, SelectionResolutionStatus status, params string[] reasonCodes)
            => new(reference, status, reasonCodes, null, null);

        private static string ScopeKey(AncillarySalesScope scope)
            => string.Join(
                '|',
                scope.OwnerAirlineId.ToString(CultureInfo.InvariantCulture),
                scope.Channel.ToString(),
                string.Join(',', scope.PointOfSaleIds.Order()),
                scope.TravelAgencyId?.ToString(CultureInfo.InvariantCulture),
                scope.AirlineOfficeId?.ToString(CultureInfo.InvariantCulture),
                scope.CustomerId?.ToString(CultureInfo.InvariantCulture),
                scope.CountryId?.ToString(CultureInfo.InvariantCulture));
    }
}
