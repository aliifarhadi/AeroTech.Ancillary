using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments
{
    public sealed record InventoryPolicyArgs(
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        FlightCountConsumption? CountConsumption,
        FlightWeightConsumption? WeightConsumption,
        AirportSlotConsumption? SlotConsumption,
        IReadOnlyList<PassengerUsageLimitArgs> PassengerUsageLimits);
}
