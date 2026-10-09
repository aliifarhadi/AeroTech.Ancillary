using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestDefineInventoryPolicyCommand(
    int OwnerAirlineId,
    string ServiceDefinitionRef,
    long ServiceDefinitionId,
    InventoryAuthority Authority,
    LocalInventoryPattern? LocalPattern = null,
    string? ProviderKey = null,
    FlightCountConsumptionInput? CountConsumption = null,
    FlightWeightConsumptionInput? WeightConsumption = null,
    AirportSlotConsumptionInput? SlotConsumption = null,
    IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits = null) : IDefineInventoryPolicyCommand;

public sealed record TestChangeInventoryPolicyCommand(
    long PolicyId,
    long ServiceDefinitionId,
    InventoryAuthority Authority,
    long ExpectedVersion,
    LocalInventoryPattern? LocalPattern = null,
    string? ProviderKey = null,
    FlightCountConsumptionInput? CountConsumption = null,
    FlightWeightConsumptionInput? WeightConsumption = null,
    AirportSlotConsumptionInput? SlotConsumption = null,
    IReadOnlyList<PassengerUsageLimitInput>? PassengerUsageLimits = null) : IChangeInventoryPolicyCommand;

public sealed record TestInventoryPolicyLifecycleCommand(long PolicyId, long ExpectedVersion)
    : IActivateInventoryPolicyCommand, ISuspendInventoryPolicyCommand, IRetireInventoryPolicyCommand;

public sealed record TestDefineFlightCountInventoryCommand(
    int OwnerAirlineId,
    long FlightId,
    long ResourceId,
    InventoryCountUnit CountUnit,
    int TotalCapacity) : IDefineFlightCountInventoryCommand;

public sealed record TestFlightCountLifecycleCommand(long InventoryId, long ExpectedVersion)
    : IActivateFlightCountInventoryCommand,
        ICloseFlightCountInventoryForSaleCommand,
        IOpenFlightCountInventoryForSaleCommand,
        ISuspendFlightCountInventoryCommand,
        IRetireFlightCountInventoryCommand;

public sealed record TestAdjustFlightCountInventoryCommand(
    long InventoryId,
    int NewTotal,
    string ReasonCode,
    string CorrelationId,
    long ExpectedVersion) : IAdjustFlightCountInventoryCommand;

public sealed record TestDefineFlightWeightInventoryCommand(
    int OwnerAirlineId,
    long FlightId,
    long WeightResourceId,
    decimal CapacityKg) : IDefineFlightWeightInventoryCommand;

public sealed record TestFlightWeightLifecycleCommand(long InventoryId, long ExpectedVersion)
    : IActivateFlightWeightInventoryCommand,
        ICloseFlightWeightInventoryForSaleCommand,
        IOpenFlightWeightInventoryForSaleCommand,
        ISuspendFlightWeightInventoryCommand,
        IRetireFlightWeightInventoryCommand;

public sealed record TestAdjustFlightWeightInventoryCommand(
    long InventoryId,
    decimal NewKg,
    string ReasonCode,
    string CorrelationId,
    long ExpectedVersion) : IAdjustFlightWeightInventoryCommand;

public sealed record TestDefineAirportSlotInventoryCommand(
    int OwnerAirlineId,
    int AirportId,
    long FacilityId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    int CapacityPersons) : IDefineAirportSlotInventoryCommand;

public sealed record TestAirportSlotLifecycleCommand(long InventoryId, long ExpectedVersion)
    : IActivateAirportSlotInventoryCommand,
        ICloseAirportSlotInventoryForSaleCommand,
        IOpenAirportSlotInventoryForSaleCommand,
        ISuspendAirportSlotInventoryCommand,
        IRetireAirportSlotInventoryCommand;

public sealed record TestAdjustAirportSlotInventoryCommand(
    long InventoryId,
    int NewTotal,
    string ReasonCode,
    string CorrelationId,
    long ExpectedVersion) : IAdjustAirportSlotInventoryCommand;

public static class P2Commands
{
    public const long PetResource = 8001;
    public const long MealResource = 8002;
    public const long OversizeResource = 8003;
    public const long BagWeightResource = 8101;
    public const long Lounge = 9001;
    public const long SecondLounge = 9002;

    public static FlightCountConsumptionInput Count(long resourceId = PetResource, int perUnit = 1, InventoryCountUnit unit = InventoryCountUnit.AnimalCarrier)
        => new(resourceId, perUnit, unit);

    public static FlightWeightConsumptionInput FixedKg(decimal kilograms, long weightResourceId = BagWeightResource)
        => new(weightResourceId, FlightWeightConsumptionMode.FixedKgPerAcceptedUnit, kilograms);

    public static FlightWeightConsumptionInput AcceptedKg(long weightResourceId = BagWeightResource)
        => new(weightResourceId, FlightWeightConsumptionMode.AcceptedWeightKg, null);

    public static AirportSlotConsumptionInput SlotUse(long facilityId = Lounge, int occupancyMinutes = 90, int peoplePerUnit = 1)
        => new(facilityId, occupancyMinutes, peoplePerUnit);

    public static PassengerUsageLimitInput Limit(PassengerUsageLimitScope scope, int maxUnits, string family) => new(scope, maxUnits, family);

    public static DateTimeOffset Utc(int hour, int minute = 0, int day = 6) => new(2027, 4, day, hour, minute, 0, TimeSpan.Zero);
}
