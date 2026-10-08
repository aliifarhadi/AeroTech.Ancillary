using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions
{
    public interface IChangeProvisionSalesRestrictionsService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionSalesRestrictionsCommand command, CancellationToken cancellationToken = default);
    }
}
