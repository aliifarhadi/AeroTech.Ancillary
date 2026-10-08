using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow
{
    public interface IChangeProvisionDayTimeWindowCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        byte DaysOfWeekMask { get; }

        TimeOnly? StartLocalTime { get; }

        TimeOnly? EndLocalTime { get; }

        DayTimeRestrictionEffect Effect { get; }
    }
}
