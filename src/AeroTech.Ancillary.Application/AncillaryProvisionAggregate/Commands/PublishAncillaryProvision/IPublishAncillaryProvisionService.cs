using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision
{
    public interface IPublishAncillaryProvisionService
    {
        Task<ProvisionResult> PublishAsync(IPublishAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
