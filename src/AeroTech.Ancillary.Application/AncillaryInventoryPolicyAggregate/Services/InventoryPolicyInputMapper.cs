using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services
{
    internal static class InventoryPolicyInputMapper
    {
        public static InventoryPolicyArgs ToArgs(
            long serviceDefinitionId,
            InventoryAuthority authority,
            LocalInventoryPattern? localPattern,
            string? providerKey,
            FlightCountConsumptionInput? countConsumption,
            FlightWeightConsumptionInput? weightConsumption,
            AirportSlotConsumptionInput? slotConsumption,
            IReadOnlyList<PassengerUsageLimitInput>? passengerUsageLimits)
            => new(
                serviceDefinitionId,
                authority,
                localPattern,
                providerKey,
                countConsumption is null
                    ? null
                    : FlightCountConsumption.Create(countConsumption.ResourceId, countConsumption.CountPerAcceptedUnit, countConsumption.CountUnit),
                ToWeight(weightConsumption),
                slotConsumption is null
                    ? null
                    : AirportSlotConsumption.Create(slotConsumption.FacilityId, slotConsumption.OccupancyMinutes, slotConsumption.PeoplePerAcceptedUnit),
                (passengerUsageLimits ?? [])
                    .Select(limit => limit is null
                        ? throw ExceptionFactory.InventoryPolicyIsInvalid(nameof(passengerUsageLimits))
                        : new PassengerUsageLimitArgs(limit.LimitScope, limit.MaxUnits, limit.CountingFamilyCode, limit.ConsumptionUnit, limit.UnitsPerPurchase))
                    .ToList());

        private static FlightWeightConsumption? ToWeight(FlightWeightConsumptionInput? weight)
        {
            if (weight is null)
                return null;

            return weight.ConsumptionMode switch
            {
                FlightWeightConsumptionMode.FixedKgPerAcceptedUnit when weight.FixedKgPerUnit is not null
                    => FlightWeightConsumption.FixedPerUnit(weight.WeightResourceId, weight.FixedKgPerUnit.Value),
                FlightWeightConsumptionMode.AcceptedWeightKg when weight.FixedKgPerUnit is null
                    => FlightWeightConsumption.AcceptedWeight(weight.WeightResourceId),
                _ => throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(FlightWeightConsumption)}.{nameof(FlightWeightConsumption.FixedKgPerUnit)}")
            };
        }
    }
}
