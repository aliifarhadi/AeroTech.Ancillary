using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction.Backoffice
{
    public sealed record BackofficeChangeProvisionDayTimeRestrictionCommand(
        long ProvisionId,
        long RowId,
        DayOfWeek DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayTimeRestrictionEffect Effect) : IRequest<ProvisionConditionRowResult>, IChangeProvisionDayTimeRestrictionCommand;
}
