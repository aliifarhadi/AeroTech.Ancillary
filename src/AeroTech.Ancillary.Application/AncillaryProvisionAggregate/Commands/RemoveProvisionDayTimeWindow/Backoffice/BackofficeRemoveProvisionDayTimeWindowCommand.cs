using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow.Backoffice
{
    public sealed record BackofficeRemoveProvisionDayTimeWindowCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionRuleRowResult>, IRemoveProvisionDayTimeWindowCommand;
}
