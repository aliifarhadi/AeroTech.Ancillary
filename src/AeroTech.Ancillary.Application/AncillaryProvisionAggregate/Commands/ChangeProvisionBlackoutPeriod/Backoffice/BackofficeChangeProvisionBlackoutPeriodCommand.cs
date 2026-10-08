using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod.Backoffice
{
    public sealed record BackofficeChangeProvisionBlackoutPeriodCommand(
        long ProvisionId,
        long RowId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionRuleRowResult>, IChangeProvisionBlackoutPeriodCommand;
}
