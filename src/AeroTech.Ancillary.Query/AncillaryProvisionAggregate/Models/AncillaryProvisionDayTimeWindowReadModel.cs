using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionDayTimeWindowReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public byte DaysOfWeekMask { get; set; }

        public TimeOnly? StartLocalTime { get; set; }

        public TimeOnly? EndLocalTime { get; set; }

        public DayTimeRestrictionEffect Effect { get; set; }
    }
}
