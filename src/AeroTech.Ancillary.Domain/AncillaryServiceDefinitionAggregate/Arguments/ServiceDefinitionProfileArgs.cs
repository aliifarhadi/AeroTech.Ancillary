using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments
{
    public sealed record ServiceDefinitionProfileArgs(
        AncillaryProfile Profile,
        string VariantCode,
        DocumentRouting DocumentRouting,
        ServiceSpecificationArgs Specification);
}
