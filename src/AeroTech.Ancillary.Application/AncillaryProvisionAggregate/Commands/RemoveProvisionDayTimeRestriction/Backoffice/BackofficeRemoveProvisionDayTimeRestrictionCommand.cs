using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction.Backoffice
{
    public sealed record BackofficeRemoveProvisionDayTimeRestrictionCommand(
        long ProvisionId,
        long RowId) : IRequest<ProvisionConditionRowResult>, IRemoveProvisionDayTimeRestrictionCommand;
}
