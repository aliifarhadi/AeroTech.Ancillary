using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice
{
    public sealed record BackofficeAddProvisionBlackoutPeriodCommand(
        long ProvisionId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionRuleRowResult>, IAddProvisionBlackoutPeriodCommand;
}
