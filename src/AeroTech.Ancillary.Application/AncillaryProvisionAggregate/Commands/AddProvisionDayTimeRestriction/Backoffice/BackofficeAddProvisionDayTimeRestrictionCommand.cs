using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction.Backoffice
{
    public sealed record BackofficeAddProvisionDayTimeRestrictionCommand(
        long ProvisionId,
        DayOfWeek DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayTimeRestrictionEffect Effect) : IRequest<ProvisionConditionRowResult>, IAddProvisionDayTimeRestrictionCommand;
}
