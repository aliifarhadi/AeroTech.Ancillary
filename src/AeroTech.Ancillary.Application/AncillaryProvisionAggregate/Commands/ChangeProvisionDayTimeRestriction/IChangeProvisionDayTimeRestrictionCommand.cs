using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction
{
    public interface IChangeProvisionDayTimeRestrictionCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        DayOfWeek DayOfWeek { get; }

        TimeOnly? StartTime { get; }

        TimeOnly? EndTime { get; }

        DayTimeRestrictionEffect Effect { get; }
    }
}
