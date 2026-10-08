using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction
{
    public interface IAddProvisionDayTimeRestrictionCommand
    {
        long ProvisionId { get; }

        DayOfWeek DayOfWeek { get; }

        TimeOnly? StartTime { get; }

        TimeOnly? EndTime { get; }

        DayTimeRestrictionEffect Effect { get; }
    }
}
