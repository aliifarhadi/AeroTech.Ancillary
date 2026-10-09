using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy.Backoffice
{
    public sealed record BackofficeChangeInventoryPolicyCommand(
        long PolicyId,
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        FlightCountConsumptionInput? CountConsumption,
        FlightWeightConsumptionInput? WeightConsumption,
        AirportSlotConsumptionInput? SlotConsumption,
        IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits,
        long ExpectedVersion) : IRequest<InventoryPolicyResult>, IChangeInventoryPolicyCommand;
}
