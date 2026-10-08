using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow
{
    public interface IAddProvisionDayTimeWindowCommand
    {
        long ProvisionId { get; }

        byte DaysOfWeekMask { get; }

        TimeOnly? StartLocalTime { get; }

        TimeOnly? EndLocalTime { get; }

        DayTimeRestrictionEffect Effect { get; }
    }
}
