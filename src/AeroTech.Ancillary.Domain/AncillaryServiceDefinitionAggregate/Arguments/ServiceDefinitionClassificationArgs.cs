namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments
{
    public sealed record ServiceDefinitionClassificationArgs(
        string? ServiceTypeCode,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code);
}
