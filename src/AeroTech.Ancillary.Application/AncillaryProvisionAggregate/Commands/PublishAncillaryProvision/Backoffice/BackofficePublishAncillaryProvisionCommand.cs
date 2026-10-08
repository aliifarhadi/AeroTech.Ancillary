using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice
{
    public sealed record BackofficePublishAncillaryProvisionCommand(
        long ProvisionId,
        long PricingId) : IRequest<ProvisionResult>, IPublishAncillaryProvisionCommand;
}
