using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        private void EnsureProfileRules(AncillaryServiceDefinition definition, ProvisionRulesArgs rules)
        {
            Require(definition.Id == ServiceDefinitionId, nameof(ServiceDefinitionId));
            EnsureProfileRule(definition, rules.PetRule is not null && definition.Pet is null, nameof(PetRule));
            EnsureProfileRule(definition, rules.AssistedTravelRule is not null && definition.AssistedTravel is null, nameof(AssistedTravelRule));
            EnsureProfileRule(definition, rules.AirportServiceRule is not null && definition.AirportService is null, nameof(AirportServiceRule));
        }

        private List<ProvisionCancellationRule>? PlanCancellationRules(
            IReadOnlyList<ProvisionCancellationRuleArgs>? rules,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces,
            IIdGenerator idGenerator)
        {
            if (rules is null)
                return null;

            if (rules.Any(rule => rule is null))
                throw ExceptionFactory.ProvisionIsInvalid(nameof(CancellationRules));

            for (var index = 1; index < rules.Count; index++)
            {
                if (rules.Take(index).Any(previous => previous.RuleRef == rules[index].RuleRef))
                    throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCancellationRule)}.{nameof(ProvisionCancellationRule.RuleRef)}");

                if (rules.Take(index).Any(previous => ProvisionCancellationRule.Overlap(previous, rules[index])))
                    throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCancellationRule)}.{nameof(ProvisionCancellationRule.BeforeStartMinutesFrom)}");
            }

            return rules
                .Select(rule => _cancellationRules.FirstOrDefault(row => row.SameAs(rule))
                                ?? new ProvisionCancellationRule(idGenerator.NewId(), Id, rule, currencyDecimalPlaces))
                .ToList();
        }

        private static void EnsureProfileRule(AncillaryServiceDefinition definition, bool foreign, string rule)
        {
            if (foreign)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(rule, definition.VariantCode);
        }

        internal static List<TRow> MergeRows<TRow>(
            List<TRow> stored,
            List<TRow> candidates,
            Func<TRow, TRow, bool> same,
            Func<TRow, TRow, bool> conflicts,
            string field)
            where TRow : class
        {
            for (var index = 1; index < candidates.Count; index++)
            {
                var candidate = candidates[index];

                if (candidates.Take(index).Any(previous => conflicts(previous, candidate)))
                    throw ExceptionFactory.ProvisionIsInvalid(field);
            }

            return candidates
                .Select(candidate => stored.FirstOrDefault(row => same(row, candidate)) ?? candidate)
                .ToList();
        }

        internal static void ReplaceRows<TRow>(List<TRow> stored, List<TRow> rows)
        {
            stored.Clear();
            stored.AddRange(rows);
        }
    }
}
