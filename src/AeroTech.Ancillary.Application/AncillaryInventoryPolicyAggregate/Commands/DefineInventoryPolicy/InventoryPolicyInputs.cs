using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public sealed record FlightCountConsumptionInput(
        long ResourceId,
        int CountPerAcceptedUnit,
        InventoryCountUnit CountUnit);

    public sealed record FlightWeightConsumptionInput(
        long WeightResourceId,
        FlightWeightConsumptionMode ConsumptionMode,
        decimal? FixedKgPerUnit);

    public sealed record AirportSlotConsumptionInput(
        long FacilityId,
        int OccupancyMinutes,
        int PeoplePerAcceptedUnit);

    public sealed record PassengerUsageLimitInput(
        PassengerUsageLimitScope LimitScope,
        int MaxUnits,
        string CountingFamilyCode);
}
