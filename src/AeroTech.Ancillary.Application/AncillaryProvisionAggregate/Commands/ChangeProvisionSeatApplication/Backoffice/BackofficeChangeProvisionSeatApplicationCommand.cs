using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication.Backoffice
{
    public sealed record BackofficeChangeProvisionSeatApplicationCommand(
        long ProvisionId,
        ProvisionSeatApplicationInput? SeatApplication) : IRequest<ProvisionResult>, IChangeProvisionSeatApplicationCommand;
}
