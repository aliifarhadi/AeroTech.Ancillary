namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public interface IDefineAncillaryProvisionService
    {
        Task<ProvisionResult> DefineAsync(IDefineAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
