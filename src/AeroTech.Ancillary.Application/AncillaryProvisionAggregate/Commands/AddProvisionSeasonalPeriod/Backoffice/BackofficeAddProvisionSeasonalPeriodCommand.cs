using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod.Backoffice
{
    public sealed record BackofficeAddProvisionSeasonalPeriodCommand(
        long ProvisionId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionConditionRowResult>, IAddProvisionSeasonalPeriodCommand;
}
