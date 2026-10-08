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

        internal bool IsEmpty => _permittedPeriods.Count == 0 && _blackoutPeriods.Count == 0;

        internal bool IsUnsatisfiable
        {
            get
            {
                var blackouts = Union(_blackoutPeriods.Select(period => (period.StartDate, period.EndDate)));

                return _permittedPeriods.Count == 0
                    ? blackouts.Any(blackout => blackout.Start == DateOnly.MinValue && blackout.End == DateOnly.MaxValue)
                    : _permittedPeriods.All(period => blackouts.Any(blackout => blackout.Start <= period.StartDate && blackout.End >= period.EndDate));
            }
        }

        internal static ProvisionTravelDateRule Open(long ancillaryProvisionId, IIdGenerator idGenerator)
            => new(idGenerator.NewId(), ancillaryProvisionId);

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

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._permittedPeriods, permittedPeriods);
                AncillaryProvision.ReplaceRows(rule._blackoutPeriods, blackoutPeriods);

                return rule;
            };
        }

        internal ProvisionPermittedTravelPeriod AddPermittedPeriod(ProvisionDatePeriodArgs period, IIdGenerator idGenerator)
        {
            var row = new ProvisionPermittedTravelPeriod(idGenerator.NewId(), AncillaryProvisionId, Id, period);

            if (_permittedPeriods.Any(other => other.SameAs(period)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            _permittedPeriods.Add(row);

            return Canonicalize(_permittedPeriods, period.StartDate);
        }

        internal ProvisionPermittedTravelPeriod ChangePermittedPeriod(long rowId, ProvisionDatePeriodArgs period)
        {
            var row = _permittedPeriods.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound();

            if (_permittedPeriods.Any(other => other.Id != rowId && other.SameAs(period)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            row.Change(period);

            return Canonicalize(_permittedPeriods, period.StartDate);
        }

        internal void RemovePermittedPeriod(long rowId)
            => _permittedPeriods.Remove(
                _permittedPeriods.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound());

        private static ProvisionPermittedTravelPeriod Canonicalize(List<ProvisionPermittedTravelPeriod> rows, DateOnly anchor)
        {
            foreach (var (start, end) in Union(rows.Select(row => (row.StartDate, row.EndDate))))
            {
                var covered = rows.Where(row => row.StartDate >= start && row.EndDate <= end).OrderBy(row => row.Id).ToList();

                covered[0].Change(new ProvisionDatePeriodArgs(start, end));

                foreach (var absorbed in covered.Skip(1))
                    rows.Remove(absorbed);
            }

            return rows.First(row => row.StartDate <= anchor && row.EndDate >= anchor);
        }

        internal ProvisionBlackoutPeriod AddBlackoutPeriod(ProvisionDatePeriodArgs period, IIdGenerator idGenerator)
        {
            var row = new ProvisionBlackoutPeriod(idGenerator.NewId(), AncillaryProvisionId, Id, period);

            if (_blackoutPeriods.Any(other => other.SameAs(period)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            _blackoutPeriods.Add(row);

            return Canonicalize(_blackoutPeriods, period.StartDate);
        }

        internal ProvisionBlackoutPeriod ChangeBlackoutPeriod(long rowId, ProvisionDatePeriodArgs period)
        {
            var row = _blackoutPeriods.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound();

            if (_blackoutPeriods.Any(other => other.Id != rowId && other.SameAs(period)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            row.Change(period);

            return Canonicalize(_blackoutPeriods, period.StartDate);
        }

        internal void RemoveBlackoutPeriod(long rowId)
            => _blackoutPeriods.Remove(
                _blackoutPeriods.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound());

        private static ProvisionBlackoutPeriod Canonicalize(List<ProvisionBlackoutPeriod> rows, DateOnly anchor)
        {
            foreach (var (start, end) in Union(rows.Select(row => (row.StartDate, row.EndDate))))
            {
                var covered = rows.Where(row => row.StartDate >= start && row.EndDate <= end).OrderBy(row => row.Id).ToList();

                covered[0].Change(new ProvisionDatePeriodArgs(start, end));

                foreach (var absorbed in covered.Skip(1))
                    rows.Remove(absorbed);
            }

            return rows.First(row => row.StartDate <= anchor && row.EndDate >= anchor);
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
