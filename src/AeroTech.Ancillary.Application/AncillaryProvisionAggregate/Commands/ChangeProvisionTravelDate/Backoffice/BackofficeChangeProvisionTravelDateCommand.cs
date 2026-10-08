using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice
{
    public sealed record BackofficeChangeProvisionTravelDateCommand(
        long ProvisionId,
        ProvisionTravelDateInput? TravelDate) : IRequest<ProvisionResult>, IChangeProvisionTravelDateCommand;
}
