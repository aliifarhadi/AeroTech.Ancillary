using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod.Backoffice
{
    public sealed record BackofficeRemoveProvisionPermittedTravelPeriodCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionRuleRowResult>, IRemoveProvisionPermittedTravelPeriodCommand;
}
