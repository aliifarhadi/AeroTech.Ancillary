using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate.Backoffice
{
    public sealed record BackofficeRemoveProvisionTravelDateCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionConditionRowResult>, IRemoveProvisionTravelDateCommand;
}
