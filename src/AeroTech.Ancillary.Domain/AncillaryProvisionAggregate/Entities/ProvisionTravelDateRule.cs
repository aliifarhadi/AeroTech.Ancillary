using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionTravelDateRule : Entity<long>
    {
        private readonly List<ProvisionPermittedTravelPeriod> _permittedPeriods = new();
        private readonly List<ProvisionBlackoutPeriod> _blackoutPeriods = new();

        private ProvisionTravelDateRule()
        {
        }

        private ProvisionTravelDateRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionPermittedTravelPeriod> PermittedPeriods => _permittedPeriods.AsReadOnly();

        public IReadOnlyCollection<ProvisionBlackoutPeriod> BlackoutPeriods => _blackoutPeriods.AsReadOnly();

        internal bool IsUnsatisfiable => Unsatisfiable(_permittedPeriods, _blackoutPeriods);

        internal static Func<ProvisionTravelDateRule?> Plan(
            ProvisionTravelDateRule? stored,
            long ancillaryProvisionId,
            ProvisionTravelDateArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionTravelDateRule(idGenerator.NewId(), ancillaryProvisionId);
            var permittedPeriods = Canonical(args.PermittedPeriods, nameof(PermittedPeriods))
                .Select(period => rule._permittedPeriods.FirstOrDefault(row => row.SameAs(period))
                                  ?? new ProvisionPermittedTravelPeriod(idGenerator.NewId(), ancillaryProvisionId, rule.Id, period))
                .ToList();
            var blackoutPeriods = Canonical(args.BlackoutPeriods, nameof(BlackoutPeriods))
                .Select(period => rule._blackoutPeriods.FirstOrDefault(row => row.SameAs(period))
                                  ?? new ProvisionBlackoutPeriod(idGenerator.NewId(), ancillaryProvisionId, rule.Id, period))
                .ToList();

            if (Unsatisfiable(permittedPeriods, blackoutPeriods))
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(AncillaryProvision.TravelDate));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._permittedPeriods, permittedPeriods);
                AncillaryProvision.ReplaceRows(rule._blackoutPeriods, blackoutPeriods);

                return rule;
            };
        }

        private static bool Unsatisfiable(
            IReadOnlyCollection<ProvisionPermittedTravelPeriod> permittedPeriods,
            IReadOnlyCollection<ProvisionBlackoutPeriod> blackoutPeriods)
        {
            var blackouts = Union(blackoutPeriods.Select(period => (period.StartDate, period.EndDate)));

            return permittedPeriods.Count == 0
                ? blackouts.Any(blackout => blackout.Start == DateOnly.MinValue && blackout.End == DateOnly.MaxValue)
                : permittedPeriods.All(period => blackouts.Any(blackout => blackout.Start <= period.StartDate && blackout.End >= period.EndDate));
        }

        private static List<ProvisionDatePeriodArgs> Canonical(IReadOnlyList<ProvisionDatePeriodArgs> periods, string field)
        {
            if (periods.Any(period => period.StartDate == DateOnly.MinValue || period.StartDate > period.EndDate)
                || periods.Distinct().Count() != periods.Count)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionTravelDateRule)}.{field}");

            return Union(periods.Select(period => (period.StartDate, period.EndDate)))
                .Select(period => new ProvisionDatePeriodArgs(period.Start, period.End))
                .ToList();
        }

        private static List<(DateOnly Start, DateOnly End)> Union(IEnumerable<(DateOnly Start, DateOnly End)> periods)
        {
            var merged = new List<(DateOnly Start, DateOnly End)>();

            foreach (var period in periods.OrderBy(period => period.Start).ThenBy(period => period.End))
            {
                if (merged.Count > 0 && (merged[^1].End == DateOnly.MaxValue || period.Start <= merged[^1].End.AddDays(1)))
                    merged[^1] = (merged[^1].Start, period.End > merged[^1].End ? period.End : merged[^1].End);
                else
                    merged.Add(period);
            }

            return merged;
        }
    }
}
