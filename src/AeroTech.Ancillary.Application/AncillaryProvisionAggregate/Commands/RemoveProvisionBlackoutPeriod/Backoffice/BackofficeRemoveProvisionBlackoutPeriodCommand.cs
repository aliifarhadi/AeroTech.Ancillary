using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice
{
    public sealed record BackofficeRemoveProvisionBlackoutPeriodCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionConditionRowResult>, IRemoveProvisionBlackoutPeriodCommand;
}
