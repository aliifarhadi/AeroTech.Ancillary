using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod.Backoffice
{
    public sealed record BackofficeAddProvisionPermittedTravelPeriodCommand(
        long ProvisionId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionRuleRowResult>, IAddProvisionPermittedTravelPeriodCommand;
}
