using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice
{
    public sealed record BackofficeChangeProvisionTravelDateCommand(
        long ProvisionId,
        long RowId,
        DateOnly TravelDate) : IRequest<ProvisionConditionRowResult>, IChangeProvisionTravelDateCommand;
}
