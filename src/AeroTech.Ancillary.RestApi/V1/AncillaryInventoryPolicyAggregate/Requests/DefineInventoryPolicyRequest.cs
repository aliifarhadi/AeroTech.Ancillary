using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryInventoryPolicyAggregate.Requests
{
    public sealed record DefineInventoryPolicyRequest(
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        FlightCountConsumptionInput? CountConsumption,
        FlightWeightConsumptionInput? WeightConsumption,
        AirportSlotConsumptionInput? SlotConsumption,
        IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits);
}
