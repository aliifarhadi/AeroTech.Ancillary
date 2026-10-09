using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class P2Fixtures
{
    public const int Airline = 1;
    public const int Ika = 2;
    public const long Definition = 1001;
    public const long Flight = 81234;
    public const long PetResource = 8001;
    public const long MealResource = 8002;
    public const long BagWeightResource = 8101;
    public const long Lounge = 9001;
    public const long Actor = 42;

    public static readonly DateTimeOffset Now = new(2026, 10, 9, 10, 0, 0, TimeSpan.Zero);

    public static InventoryPolicyArgs Args(
        InventoryAuthority authority,
        LocalInventoryPattern? pattern = null,
        string? providerKey = null,
        FlightCountConsumption? count = null,
        FlightWeightConsumption? weight = null,
        AirportSlotConsumption? slot = null,
        params PassengerUsageLimitArgs[] limits)
        => new(Definition, authority, pattern, providerKey, count, weight, slot, limits);

    public static AncillaryInventoryPolicy Policy(InventoryPolicyArgs args, string reference = "PET_IN_CABIN", long id = 6001, int airline = Airline)
        => AncillaryInventoryPolicy.Define(id, airline, reference, args, new SequentialIdGenerator(), Now);

    public static FlightCountConsumption Count(long resourceId = PetResource, int perUnit = 1, InventoryCountUnit unit = InventoryCountUnit.AnimalCarrier)
        => FlightCountConsumption.Create(resourceId, perUnit, unit);

    public static InventoryPolicyEvidence Evidence(
        PricingUnit? pricingUnit = PricingUnit.PerItem,
        bool identity = true,
        string? supplierProviderKey = null,
        InventoryReferenceCheck flightFlow = InventoryReferenceCheck.SourceUnavailable,
        InventoryReferenceCheck countResource = InventoryReferenceCheck.SourceUnavailable,
        InventoryCountUnit? countUnitOfLiveSources = null,
        InventoryReferenceCheck weightResource = InventoryReferenceCheck.SourceUnavailable,
        InventoryReferenceCheck slotFacility = InventoryReferenceCheck.SourceUnavailable,
        params (string Code, InventoryReferenceCheck Check)[] families)
        => new(
            identity ? Definition : null,
            pricingUnit,
            supplierProviderKey,
            flightFlow,
            countResource,
            countUnitOfLiveSources,
            weightResource,
            slotFacility,
            families.ToDictionary(family => family.Code, family => family.Check));

    public static FlightSourceEvidence Verified(InventoryCountUnit? unitOfLiveSources = null)
        => new(InventoryReferenceCheck.Verified, InventoryReferenceCheck.Verified, unitOfLiveSources);

    public static FlightCountInventory CountSource(
        int total = 4,
        long flightId = Flight,
        long resourceId = PetResource,
        InventoryCountUnit unit = InventoryCountUnit.AnimalCarrier,
        long id = 7001,
        int airline = Airline)
        => FlightCountInventory.Define(id, airline, flightId, resourceId, unit, total, Now);

    public static FlightWeightInventory WeightSource(decimal capacityKg = 100m, long flightId = Flight, long resourceId = BagWeightResource, long id = 7101)
        => FlightWeightInventory.Define(id, Airline, flightId, resourceId, capacityKg, Now);

    public static AirportSlotInventory Slot(int fromHour, int toHour, int capacity = 20, long facilityId = Lounge, long id = 7201, int fromMinute = 0, int toMinute = 0)
        => AirportSlotInventory.Define(
            id,
            Airline,
            Ika,
            facilityId,
            new DateTimeOffset(2027, 4, 6, fromHour, fromMinute, 0, TimeSpan.Zero),
            new DateTimeOffset(2027, 4, 6, toHour, toMinute, 0, TimeSpan.Zero),
            capacity,
            Now);

    public static AirportSlotEvidence SlotVerified(int airportId = Ika, string timeZoneId = "Asia/Tehran")
        => new(true, InventoryReferenceCheck.Verified, airportId, timeZoneId);
}
