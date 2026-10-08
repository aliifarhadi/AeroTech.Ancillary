using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    internal static class ServiceDefinitionResultFactory
    {
        public static ServiceDefinitionResult ToResult(this AncillaryServiceDefinition definition)
            => new(
                definition.Id,
                definition.OwnerAirlineId,
                definition.SupplierId,
                definition.ServiceDefinitionRef,
                definition.Version,
                definition.ServiceTypeCode,
                definition.ServiceSubCode,
                definition.SubCodeSource,
                definition.GroupCode,
                definition.PricingUnit,
                definition.Status);
    }
}
