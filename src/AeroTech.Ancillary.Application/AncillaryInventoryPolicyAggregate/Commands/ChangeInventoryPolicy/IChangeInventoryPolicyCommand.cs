using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy
{
    public interface IChangeInventoryPolicyCommand
    {
        long PolicyId { get; }

        long ServiceDefinitionId { get; }

        InventoryAuthority Authority { get; }

        LocalInventoryPattern? LocalPattern { get; }

        string? ProviderKey { get; }

        FlightCountConsumptionInput? CountConsumption { get; }

        FlightWeightConsumptionInput? WeightConsumption { get; }

        AirportSlotConsumptionInput? SlotConsumption { get; }

        IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits { get; }

        long ExpectedVersion { get; }
    }
}
