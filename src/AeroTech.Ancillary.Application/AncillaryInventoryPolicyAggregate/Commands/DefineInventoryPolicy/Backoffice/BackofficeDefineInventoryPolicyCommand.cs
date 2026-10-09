using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy.Backoffice
{
    public sealed record BackofficeDefineInventoryPolicyCommand(
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        FlightCountConsumptionInput? CountConsumption,
        FlightWeightConsumptionInput? WeightConsumption,
        AirportSlotConsumptionInput? SlotConsumption,
        IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits) : IRequest<InventoryPolicyResult>, IDefineInventoryPolicyCommand;
}
