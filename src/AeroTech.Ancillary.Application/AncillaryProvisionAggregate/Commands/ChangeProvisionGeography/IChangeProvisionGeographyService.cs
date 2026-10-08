using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography
{
    public interface IChangeProvisionGeographyService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionGeographyCommand command, CancellationToken cancellationToken = default);
    }
}
