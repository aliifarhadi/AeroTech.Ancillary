using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionDayTimeWindowArgs(
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect);
}
