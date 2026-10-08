using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record ProvisionDayTimeWindowRequest(
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect);
}
