using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public interface IDefineInventoryPolicyCommand
    {
        int OwnerAirlineId { get; }

        string ServiceDefinitionRef { get; }

        long ServiceDefinitionId { get; }

        InventoryAuthority Authority { get; }

        LocalInventoryPattern? LocalPattern { get; }

        string? ProviderKey { get; }

        FlightCountConsumptionInput? CountConsumption { get; }

        FlightWeightConsumptionInput? WeightConsumption { get; }

        AirportSlotConsumptionInput? SlotConsumption { get; }

        IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits { get; }
    }
}
