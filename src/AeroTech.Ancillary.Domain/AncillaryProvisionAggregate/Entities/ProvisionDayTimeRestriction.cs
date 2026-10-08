using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionDayTimeRestriction : Entity<long>
    {
        private ProvisionDayTimeRestriction()
        {
        }

        internal ProvisionDayTimeRestriction(long id, long ancillaryProvisionId, ProvisionDayTimeRestrictionArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public DayOfWeek DayOfWeek { get; private set; }

        public TimeOnly? StartTime { get; private set; }

        public TimeOnly? EndTime { get; private set; }

        public DayTimeRestrictionEffect Effect { get; private set; }

        internal void Change(ProvisionDayTimeRestrictionArgs args)
        {
            Require(Enum.IsDefined(args.DayOfWeek), nameof(DayOfWeek));
            Require(args.StartTime is null == args.EndTime is null, nameof(EndTime));
            Require(args.StartTime is null || args.StartTime <= args.EndTime, nameof(EndTime));
            Require(Enum.IsDefined(args.Effect), nameof(Effect));

            DayOfWeek = args.DayOfWeek;
            StartTime = args.StartTime;
            EndTime = args.EndTime;
            Effect = args.Effect;
        }

        internal bool SameAs(ProvisionDayTimeRestriction other)
            => DayOfWeek == other.DayOfWeek
               && StartTime == other.StartTime
               && EndTime == other.EndTime
               && Effect == other.Effect;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionDayTimeRestriction)}.{field}");
        }
    }
}
