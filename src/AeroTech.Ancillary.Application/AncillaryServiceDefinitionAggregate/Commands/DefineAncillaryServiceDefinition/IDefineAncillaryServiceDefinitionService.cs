namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public interface IDefineAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> DefineAsync(IDefineAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
