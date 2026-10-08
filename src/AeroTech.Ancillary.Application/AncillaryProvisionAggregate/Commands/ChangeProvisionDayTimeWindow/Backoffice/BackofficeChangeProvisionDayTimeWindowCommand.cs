using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow.Backoffice
{
    public sealed record BackofficeChangeProvisionDayTimeWindowCommand(
        long ProvisionId,
        long RowId,
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect) : IRequest<ProvisionRuleRowResult>, IChangeProvisionDayTimeWindowCommand;
}
