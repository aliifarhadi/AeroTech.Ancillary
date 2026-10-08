using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record ProvisionDayTimeRestrictionRequest(
        DayOfWeek DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayTimeRestrictionEffect Effect);
}
