using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication.Backoffice
{
    public sealed record BackofficeChangeProvisionDayTimeApplicationCommand(
        long ProvisionId,
        ProvisionDayTimeApplicationInput? DayTimeApplication) : IRequest<ProvisionResult>, IChangeProvisionDayTimeApplicationCommand;
}
