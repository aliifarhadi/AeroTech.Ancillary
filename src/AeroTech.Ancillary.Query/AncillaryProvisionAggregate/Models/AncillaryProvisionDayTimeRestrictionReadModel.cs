using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionDayTimeRestrictionReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public DayTimeRestrictionEffect Effect { get; set; }
    }
}
