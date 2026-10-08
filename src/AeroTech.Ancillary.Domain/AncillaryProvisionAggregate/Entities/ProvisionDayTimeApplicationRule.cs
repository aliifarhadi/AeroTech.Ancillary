using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionDayTimeApplicationRule : Entity<long>
    {
        private const int DaysInWeek = 7;

        private readonly List<ProvisionDayTimeWindow> _windows = new();

        private ProvisionDayTimeApplicationRule()
        {
        }

        private ProvisionDayTimeApplicationRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionDayTimeWindow> Windows => _windows.AsReadOnly();

        internal bool IsEmpty => _windows.Count == 0;

        internal bool IsUnsatisfiable => Enumerable.Range(0, DaysInWeek).All(day => !HasOpenTime((byte)(1 << day)));

        internal static ProvisionDayTimeApplicationRule Open(long ancillaryProvisionId, IIdGenerator idGenerator)
            => new(idGenerator.NewId(), ancillaryProvisionId);

        internal static Func<ProvisionDayTimeApplicationRule?> Plan(
            ProvisionDayTimeApplicationRule? stored,
            long ancillaryProvisionId,
            ProvisionDayTimeApplicationArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            if (args.Windows.Distinct().Count() != args.Windows.Count)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionDayTimeApplicationRule)}.{nameof(Windows)}");

            var rule = stored ?? new ProvisionDayTimeApplicationRule(idGenerator.NewId(), ancillaryProvisionId);
            var windows = args.Windows
                .Select(window => rule._windows.FirstOrDefault(row => row.SameAs(window))
                                  ?? new ProvisionDayTimeWindow(idGenerator.NewId(), ancillaryProvisionId, rule.Id, window))
                .ToList();

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._windows, windows);

                return rule;
            };
        }

        internal ProvisionDayTimeWindow AddWindow(ProvisionDayTimeWindowArgs window, IIdGenerator idGenerator)
        {
            var row = new ProvisionDayTimeWindow(idGenerator.NewId(), AncillaryProvisionId, Id, window);

            if (_windows.Any(other => other.SameAs(window)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            _windows.Add(row);

            return row;
        }

        internal ProvisionDayTimeWindow ChangeWindow(long rowId, ProvisionDayTimeWindowArgs window)
        {
            var row = _windows.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound();

            if (_windows.Any(other => other.Id != rowId && other.SameAs(window)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();

            row.Change(window);

            return row;
        }

        internal void RemoveWindow(long rowId)
            => _windows.Remove(_windows.FirstOrDefault(other => other.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound());

        private bool HasOpenTime(byte day)
        {
            var allows = _windows.Any(window => window.Effect == DayTimeRestrictionEffect.Allow)
                ? _windows.Where(window => window.Effect == DayTimeRestrictionEffect.Allow && window.AppliesOn(day)).Select(window => window.Span).ToList()
                : [(Start: 0L, End: TimeSpan.TicksPerDay)];
            var denies = _windows
                .Where(window => window.Effect == DayTimeRestrictionEffect.Deny && window.AppliesOn(day))
                .Select(window => window.Span)
                .OrderBy(span => span.Start)
                .ToList();

            return allows.Any(allow => IsOpen(allow, denies));
        }

        private static bool IsOpen((long Start, long End) allow, List<(long Start, long End)> denies)
        {
            var cursor = allow.Start;

            foreach (var deny in denies)
            {
                if (deny.End <= cursor)
                    continue;

                if (deny.Start > cursor)
                    return true;

                cursor = deny.End;

                if (cursor >= allow.End)
                    return false;
            }

            return cursor < allow.End;
        }
    }
}
