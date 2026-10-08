using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow.Backoffice
{
    public sealed record BackofficeAddProvisionDayTimeWindowCommand(
        long ProvisionId,
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect) : IRequest<ProvisionRuleRowResult>, IAddProvisionDayTimeWindowCommand;
}
