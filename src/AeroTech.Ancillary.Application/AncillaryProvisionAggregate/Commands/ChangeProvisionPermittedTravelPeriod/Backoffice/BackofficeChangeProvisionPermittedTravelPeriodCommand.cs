using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod.Backoffice
{
    public sealed record BackofficeChangeProvisionPermittedTravelPeriodCommand(
        long ProvisionId,
        long RowId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionRuleRowResult>, IChangeProvisionPermittedTravelPeriodCommand;
}
