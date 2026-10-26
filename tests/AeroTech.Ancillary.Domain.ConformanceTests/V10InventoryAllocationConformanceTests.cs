using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P2Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V10InventoryAllocationConformanceTests
{
    private static readonly InventoryReferenceCheck Ok = InventoryReferenceCheck.Verified;
    private static readonly DateTimeOffset Appointment = new(2027, 4, 6, 8, 30, 0, TimeSpan.Zero);
    private static readonly InventorySourceKey CountKey = new(InventoryResourceKind.FlightCount, 7001);
    private static readonly InventorySourceKey WeightKey = new(InventoryResourceKind.FlightWeight, 7101);

    private static AncillaryInventoryPolicy Active(InventoryPolicyArgs args, InventoryPolicyEvidence evidence)
    {
        var policy = Policy(args);
        policy.Activate(evidence, 1, Now);
        return policy;
    }

    private static AncillaryInventoryPolicy CountPolicy()
        => Active(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count()), Evidence(countResource: Ok));

    private static AncillaryInventoryPolicy SlotPolicy()
        => Active(
            Args(InventoryAuthority.Local, LocalInventoryPattern.AirportSlot, slot: AirportSlotConsumption.Create(Lounge, 90, 1)),
            Evidence(PricingUnit.PerPassenger, slotFacility: Ok));

    private static AncillaryReservationUnitArgs Unit(long orderServiceId = 9001, int quantity = 1, params AncillaryReservationAllocationArgs[] allocations)
        => M1Fixtures.Unit(orderServiceId, quantity: quantity) with { Allocations = allocations };

    private static AncillaryReservationAllocationArgs Uses(InventorySourceKey source, decimal amount)
        => new(source.ResourceKind, source.InventorySourceId, amount);

    private static Dictionary<InventorySourceKey, InventorySourceBalance> Balances(
        decimal countCapacity = 4,
        decimal countUsage = 0,
        decimal weightCapacity = 100m,
        decimal weightUsage = 0m,
        InventoryRecordStatus status = InventoryRecordStatus.Active,
        bool closedForSale = false)
        => new()
        {
            [CountKey] = new InventorySourceBalance(countCapacity, status, closedForSale, countUsage),
            [WeightKey] = new InventorySourceBalance(weightCapacity, InventoryRecordStatus.Active, false, weightUsage),
        };

    [Fact]
    public void V10_G2_D01_a_hold_accepts_only_an_active_definition_and_provision_that_need_no_supplier_confirmation()
    {
        var supplier = M1Fixtures.LocalSupplier();

        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.Accept(M1Fixtures.Unit(), M1Fixtures.LoungeDefinition(), M1Fixtures.ActiveLoungeProvision(), supplier));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.Accept(M1Fixtures.Unit(), M1Fixtures.ActiveLoungeDefinition(), M1Fixtures.LoungeProvision(), supplier));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.Accept(
            M1Fixtures.Unit(),
            M1Fixtures.ActiveLoungeDefinition(confirmation: ConfirmationRequirement.SubjectToConfirmation),
            M1Fixtures.ActiveLoungeProvision(),
            supplier));

        AncillaryHoldChecks.Accept(M1Fixtures.Unit(), M1Fixtures.ActiveLoungeDefinition(), M1Fixtures.ActiveLoungeProvision(), supplier);
    }

    [Fact]
    public void V10_I01_I02_I03_I04_only_an_active_unlimited_or_local_policy_without_usage_limits_is_reservable()
    {
        var definition = M1Fixtures.ActiveLoungeDefinition();
        var draft = Policy(Args(InventoryAuthority.Unlimited));
        var supplier = Active(Args(InventoryAuthority.Supplier, providerKey: "LoungePartnerA"), Evidence(supplierProviderKey: "LoungePartnerA"));
        var limited = Active(
            Args(InventoryAuthority.Unlimited, limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerOrder, 1, "LOUNGE")),
            Evidence(families: ("LOUNGE", Ok)));

        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, null));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, draft));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, supplier));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, limited));

        AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, Active(Args(InventoryAuthority.Unlimited), Evidence()));
        AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, CountPolicy());
    }

    [Fact]
    public void V10_I12_accepted_weight_consumption_is_refused_before_any_allocation()
    {
        var byWeight = Active(
            Args(InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, weight: FlightWeightConsumption.AcceptedWeight(BagWeightResource)),
            Evidence(PricingUnit.PerKilogram, weightResource: Ok));

        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), M1Fixtures.ActiveLoungeDefinition(), byWeight));
    }

    [Fact]
    public void V10_G2_D02_inventory_facts_belong_only_to_a_slot_policy_and_flight_stock_needs_the_covered_flights()
    {
        var definition = M1Fixtures.ActiveLoungeDefinition();

        BusinessAssert.Throws(16402, 422, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit() with { AppointmentTimeWithOffset = Appointment }, definition, CountPolicy()));
        BusinessAssert.Throws(16402, 422, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit() with { GuestCount = 1 }, definition, CountPolicy()));
        BusinessAssert.Throws(16402, 422, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit(), definition, SlotPolicy()));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit() with { AppointmentTimeWithOffset = Appointment }, P1Fixtures.CarrierDefinition(), SlotPolicy()));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureReservable(
            M1Fixtures.Unit(coverageScope: ServiceCoverageScope.Order, coveredFlightIds: []),
            definition,
            CountPolicy()));

        Assert.NotNull(definition.AirportService);
        AncillaryHoldChecks.EnsureReservable(M1Fixtures.Unit() with { AppointmentTimeWithOffset = Appointment, GuestCount = 2 }, definition, SlotPolicy());
    }

    [Fact]
    public void V10_I06_I11_I20_demand_is_quantity_times_the_binding_and_guests_count_as_persons()
    {
        Assert.Equal(6, Count(perUnit: 3).RequiredUnits(2));
        Assert.Equal(37.5m, FlightWeightConsumption.FixedPerUnit(BagWeightResource, 12.5m).RequiredKg(3, null));
        Assert.Equal(0.003m, FlightWeightConsumption.FixedPerUnit(BagWeightResource, 0.001m).RequiredKg(3, null));
        Assert.Equal(6, AirportSlotConsumption.Create(Lounge, 90, 2).RequiredPersons(2, 1));
        Assert.Equal(2, AirportSlotConsumption.Create(Lounge, 90, 2).RequiredPersons(1, 0));

        BusinessAssert.Throws(16602, 422, () => AirportSlotConsumption.Create(Lounge, 90, 2).RequiredPersons(int.MaxValue, 0));
        BusinessAssert.Throws(16602, 422, () => AirportSlotConsumption.Create(Lounge, 90, 1).RequiredPersons(1, -1));
        BusinessAssert.Throws(16602, 422, () => Count(perUnit: 2).RequiredUnits(int.MaxValue));
    }

    [Fact]
    public void V10_I17_I18_the_appointment_must_be_covered_by_contiguous_slots()
    {
        var end = Appointment.AddMinutes(90);

        AncillaryHoldChecks.EnsureCovered([Slot(9, 11, id: 7202), Slot(8, 9)], Appointment, end);
        AncillaryHoldChecks.EnsureCovered([Slot(8, 10)], Appointment, end);

        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureCovered([], Appointment, end));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureCovered([Slot(8, 9), Slot(9, 10, id: 7202, fromMinute: 15)], Appointment, end));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureCovered([Slot(8, 9, fromMinute: 45), Slot(9, 11, id: 7202)], Appointment, end));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureCovered([Slot(8, 9), Slot(9, 9, id: 7202, toMinute: 30)], Appointment, end));
    }

    [Fact]
    public void V10_I07_I08_I14_demand_is_summed_per_source_across_units_and_any_shortage_refuses_the_whole_hold()
    {
        AncillaryHoldChecks.EnsureAvailable([Unit(9001, 1, Uses(CountKey, 1)), Unit(9002, 1, Uses(CountKey, 1))], Balances(countUsage: 2));
        AncillaryHoldChecks.EnsureAvailable([Unit(9001, 1, Uses(WeightKey, 0.125m))], Balances(weightCapacity: 10.125m, weightUsage: 10m));
        AncillaryHoldChecks.EnsureAvailable([Unit()], new Dictionary<InventorySourceKey, InventorySourceBalance>());

        BusinessAssert.Throws(16417, 409, () => AncillaryHoldChecks.EnsureAvailable(
            [Unit(9001, 1, Uses(CountKey, 1)), Unit(9002, 1, Uses(CountKey, 1))],
            Balances(countUsage: 3)));
        BusinessAssert.Throws(16417, 409, () => AncillaryHoldChecks.EnsureAvailable(
            [Unit(9001, 1, Uses(CountKey, 1), Uses(WeightKey, 0.001m))],
            Balances(weightCapacity: 10.125m, weightUsage: 10.125m)));
    }

    [Fact]
    public void V10_I19_a_missing_closed_or_inactive_source_is_not_open_for_sale()
    {
        var demand = new[] { Unit(9001, 1, Uses(CountKey, 1)) };

        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureAvailable(demand, new Dictionary<InventorySourceKey, InventorySourceBalance>()));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureAvailable(demand, Balances(closedForSale: true)));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureAvailable(demand, Balances(status: InventoryRecordStatus.Suspended)));
        BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.EnsureAvailable(demand, Balances(status: InventoryRecordStatus.Retired)));
    }

    [Fact]
    public void V10_I24_capacity_cannot_be_adjusted_below_the_active_usage_of_the_source()
    {
        var ids = new SequentialIdGenerator();
        var count = CountSource(4);
        var weight = WeightSource(20m);
        var slot = Slot(8, 9, 20);

        BusinessAssert.Throws(16621, 409, () => count.AdjustTo(2, 3, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16621, 409, () => weight.AdjustTo(10m, 10.001m, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16621, 409, () => slot.AdjustTo(4, 5, "MAINTENANCE", Actor, "corr-1", 1, ids, Now));

        Assert.Equal((4, 3), (count.AdjustTo(3, 3, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now).PreviousTotal, count.TotalCapacity));
        Assert.Equal(10.001m, weight.AdjustTo(10.001m, 10.001m, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now).NewKg);
        Assert.Equal(5, slot.AdjustTo(5, 5, "MAINTENANCE", Actor, "corr-1", 1, ids, Now).NewTotal);
    }
}
