using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public sealed record ServiceDefinitionResult(
        long Id,
        int OwnerAirlineId,
        long SupplierId,
        string ServiceDefinitionRef,
        int Version,
        string ServiceTypeCode,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string GroupCode,
        PricingUnit? PricingUnit,
        ServiceDefinitionStatus Status);
}
