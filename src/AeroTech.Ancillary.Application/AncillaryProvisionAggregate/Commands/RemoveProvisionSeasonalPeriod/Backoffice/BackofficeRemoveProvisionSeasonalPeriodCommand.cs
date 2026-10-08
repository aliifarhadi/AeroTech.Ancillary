using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod.Backoffice
{
    public sealed record BackofficeRemoveProvisionSeasonalPeriodCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionConditionRowResult>, IRemoveProvisionSeasonalPeriodCommand;
}
