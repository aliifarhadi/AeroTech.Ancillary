using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod.Backoffice
{
    public sealed record BackofficeChangeProvisionSeasonalPeriodCommand(
        long ProvisionId,
        long RowId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionConditionRowResult>, IChangeProvisionSeasonalPeriodCommand;
}
