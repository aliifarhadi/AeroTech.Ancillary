using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionDayTimeWindow : Entity<long>
    {
        private const byte EveryDay = 127;

        private ProvisionDayTimeWindow()
        {
        }

        internal ProvisionDayTimeWindow(long id, long ancillaryProvisionId, long provisionDayTimeApplicationRuleId, ProvisionDayTimeWindowArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionDayTimeApplicationRuleId = provisionDayTimeApplicationRuleId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionDayTimeApplicationRuleId { get; private set; }

        public byte DaysOfWeekMask { get; private set; }

        public TimeOnly? StartLocalTime { get; private set; }

        public TimeOnly? EndLocalTime { get; private set; }

        public DayTimeRestrictionEffect Effect { get; private set; }

        internal (long Start, long End) Span => (StartLocalTime?.Ticks ?? 0, EndLocalTime?.Ticks ?? TimeSpan.TicksPerDay);

        internal void Change(ProvisionDayTimeWindowArgs args)
        {
            Require(args.DaysOfWeekMask is >= 1 and <= EveryDay, nameof(DaysOfWeekMask));
            Require(args.EndLocalTime is null || args.EndLocalTime > TimeOnly.MinValue, nameof(EndLocalTime));
            Require(
                args.StartLocalTime is null || args.EndLocalTime is null || args.StartLocalTime < args.EndLocalTime,
                nameof(EndLocalTime));
            Require(Enum.IsDefined(args.Effect), nameof(Effect));

            DaysOfWeekMask = args.DaysOfWeekMask;
            StartLocalTime = args.StartLocalTime;
            EndLocalTime = args.EndLocalTime;
            Effect = args.Effect;
        }

        internal bool SameAs(ProvisionDayTimeWindowArgs args)
            => DaysOfWeekMask == args.DaysOfWeekMask
               && StartLocalTime == args.StartLocalTime
               && EndLocalTime == args.EndLocalTime
               && Effect == args.Effect;

        internal bool AppliesOn(byte day) => (DaysOfWeekMask & day) != 0;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionDayTimeWindow)}.{field}");
        }
    }
}
