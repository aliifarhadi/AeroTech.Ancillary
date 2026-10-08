using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication.Backoffice
{
    public sealed record BackofficeChangeProvisionFlightApplicationCommand(
        long ProvisionId,
        ProvisionFlightApplicationInput? FlightApplication) : IRequest<ProvisionResult>, IChangeProvisionFlightApplicationCommand;
}
