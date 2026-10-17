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

        internal bool IsUnsatisfiable => Unsatisfiable(_windows);

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

            if (Unsatisfiable(windows))
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(AncillaryProvision.DayTimeApplication));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._windows, windows);

                return rule;
            };
        }

        private static bool Unsatisfiable(IReadOnlyCollection<ProvisionDayTimeWindow> windows)
            => Enumerable.Range(0, DaysInWeek).All(day => !HasOpenTime(windows, (byte)(1 << day)));

        private static bool HasOpenTime(IReadOnlyCollection<ProvisionDayTimeWindow> windows, byte day)
        {
            var allows = windows.Any(window => window.Effect == DayTimeRestrictionEffect.Allow)
                ? windows.Where(window => window.Effect == DayTimeRestrictionEffect.Allow && window.AppliesOn(day)).Select(window => window.Span).ToList()
                : [(Start: 0L, End: TimeSpan.TicksPerDay)];
            var denies = windows
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
