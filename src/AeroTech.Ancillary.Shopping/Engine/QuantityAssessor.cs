using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class QuantityAssessor
    {
        public static async Task<QuantityAssessment> AssessAsync(
            AncillaryServiceDefinition definition,
            AncillaryProvision provision,
            AncillaryInventoryPolicy? policy,
            EvaluationUnit unit,
            UnitTimeline timeline,
            InventoryReferenceProbe references,
            CancellationToken cancellationToken)
        {
            var reasons = new List<string>();
            var limits = new List<CumulativeLimitAssessment>();
            var quantity = unit.Selection?.Quantity;

            foreach (var limit in policy is { Status: InventoryRecordStatus.Active } ? policy.PassengerUsageLimits : [])
            {
                var verified = await references.FamilyAsync(limit.CountingFamilyCode, cancellationToken) == InventoryReferenceCheck.Verified;
                var consumed = Consumed(limit, unit, timeline);

                if (!verified)
                    reasons.Add(ShoppingReasonCodes.CountingFamilyNotVerified);

                if (consumed is null)
                    reasons.Add(ShoppingReasonCodes.UsageNotVerified);

                decimal? remaining = consumed is null ? null : Math.Max(limit.MaxUnits - consumed.Value, 0m);
                var requested = quantity is null
                    ? (decimal?)null
                    : limit.ConsumptionUnit == UsageConsumptionUnit.Kilogram ? quantity.Value * (limit.UnitsPerPurchase ?? 1m) : quantity.Value;

                if (remaining is not null && requested > remaining)
                    reasons.Add(ShoppingReasonCodes.QuantityExceedsVerifiedRemaining);

                limits.Add(new CumulativeLimitAssessment(
                    limit.CountingFamilyCode,
                    limit.LimitScope,
                    limit.MaxUnits,
                    limit.ConsumptionUnit,
                    limit.UnitsPerPurchase,
                    verified,
                    consumed,
                    remaining));
            }

            return new QuantityAssessment(
                provision.Quantity.Unit,
                provision.Quantity.MinQuantity,
                provision.Quantity.MaxQuantity,
                definition.SelectionContract!.ZeroQuantityMeansNoSelection,
                quantity,
                limits,
                reasons.Distinct(StringComparer.Ordinal).ToList());
        }

        private static decimal? Consumed(PassengerUsageLimit limit, EvaluationUnit unit, UnitTimeline timeline)
        {
            var keys = Keys(limit, unit, timeline);

            if (keys is null)
                return null;

            decimal consumed = 0m;

            foreach (var key in keys)
            {
                var evidence = unit.Context.UsageEvidence.FirstOrDefault(row => row.UsageSubjectKey == key && row.IsCompleteForScope);

                if (evidence is null)
                    return null;

                consumed = Math.Max(consumed, evidence.UnitsConsumed);
            }

            return consumed;
        }

        private static IReadOnlyList<PassengerUsageKey>? Keys(PassengerUsageLimit limit, EvaluationUnit unit, UnitTimeline timeline)
        {
            var traveller = unit.Traveller;
            var identity = string.IsNullOrWhiteSpace(traveller.StableTravellerIdentity) ? null : traveller.StableTravellerIdentity;
            var orderId = unit.Context.SourceIdentity.OrderId;

            switch (limit.LimitScope)
            {
                case PassengerUsageLimitScope.PerOrder:
                    return orderId is > 0 && traveller.OrderTravellerId is > 0
                        ? [limit.KeyFor(new PassengerUsageSubject(null, orderId, traveller.OrderTravellerId, null, null))]
                        : null;
                case PassengerUsageLimitScope.PerFlightOccurrence:
                    return identity is null
                        ? null
                        : unit.Flights.Select(flight => limit.KeyFor(new PassengerUsageSubject(identity, null, null, flight.FlightId, null))).ToList();
                case PassengerUsageLimitScope.PerPortion:
                    return identity is null
                        ? null
                        : unit.Portions.Select(portion => limit.KeyFor(new PassengerUsageSubject(identity, null, null, null, null, portion.PortionRef))).ToList();
                default:
                    return identity is null || timeline.Occurrences is null
                        ? null
                        : timeline.Occurrences
                            .Select(occurrence => occurrence.LocalDate)
                            .Distinct()
                            .Select(date => limit.KeyFor(new PassengerUsageSubject(identity, null, null, null, date)))
                            .ToList();
            }
        }
    }
}
