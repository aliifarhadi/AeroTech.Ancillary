using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P2Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P2InventoryPolicyConformanceTests
{
    private static readonly InventoryReferenceCheck Ok = InventoryReferenceCheck.Verified;

    [Fact]
    public void P2_C01_unlimited_policy_has_no_binding_pattern_or_provider_and_activates_without_any_counter()
    {
        var policy = Policy(Args(InventoryAuthority.Unlimited), "INS_TRAVEL_BASIC");

        Assert.Equal((InventoryRecordStatus.Draft, 1L, InventoryAuthority.Unlimited), (policy.Status, policy.Version, policy.Authority));
        Assert.Null(policy.LocalPattern);
        Assert.Null(policy.ProviderKey);
        Assert.Null(policy.CountConsumption);
        Assert.Null(policy.WeightConsumption);
        Assert.Null(policy.SlotConsumption);
        Assert.Empty(policy.PassengerUsageLimits);
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, count: Count())));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, LocalInventoryPattern.FlightCount)));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, providerKey: "InsurancePartnerA")));
        BusinessAssert.Throws(16602, 422, () => Policy(Args((InventoryAuthority)9)));

        policy.Activate(Evidence(PricingUnit.PerPassenger), 1, Now.AddMinutes(1));

        Assert.Equal((InventoryRecordStatus.Active, 2L, Now.AddMinutes(1)), (policy.Status, policy.Version, policy.ActivatedAt!.Value));
        Assert.DoesNotContain(
            typeof(AncillaryInventoryPolicy).GetProperties(),
            property => property.Name.Contains("Capacity", StringComparison.Ordinal) || property.Name.Contains("Total", StringComparison.Ordinal) || property.Name.Contains("Quantity", StringComparison.Ordinal));
    }

    [Fact]
    public void P2_C01_X09_an_unlimited_policy_needs_only_a_resolvable_service_identity()
    {
        var policy = Policy(Args(InventoryAuthority.Unlimited));

        Assert.DoesNotContain(typeof(InventoryPolicyEvidence).GetProperties(), property => property.Name.Contains("MustCheck", StringComparison.Ordinal));
        BusinessAssert.Throws(16617, 422, () => policy.Activate(Evidence(identity: false), 1, Now));
        Assert.Equal((InventoryRecordStatus.Draft, 1L), (policy.Status, policy.Version));

        policy.Activate(Evidence(), 1, Now);

        Assert.Equal((InventoryRecordStatus.Active, 2L, Definition), (policy.Status, policy.Version, policy.ServiceDefinitionId));
    }

    [Fact]
    public void P2_C03_H05_supplier_policy_needs_the_recorded_provider_key_and_owns_no_local_binding()
    {
        var policy = Policy(Args(InventoryAuthority.Supplier, providerKey: "HotelPartnerA"), "HOTEL_ROOM_STD");

        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Supplier, providerKey: "HotelPartnerA", count: Count())));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Supplier, LocalInventoryPattern.RoomNight, "HotelPartnerA")));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Supplier, providerKey: "Hotel Partner")));
        BusinessAssert.Throws(16606, 409, () => Policy(Args(InventoryAuthority.Supplier)).Activate(Evidence(supplierProviderKey: "HotelPartnerA"), 1, Now));
        BusinessAssert.Throws(16606, 409, () => policy.Activate(Evidence(), 1, Now));
        BusinessAssert.Throws(16606, 409, () => policy.Activate(Evidence(supplierProviderKey: "OtherPartner"), 1, Now));

        policy.Activate(Evidence(supplierProviderKey: "HotelPartnerA"), 1, Now);

        Assert.Equal((InventoryRecordStatus.Active, "HotelPartnerA"), (policy.Status, policy.ProviderKey));
        Assert.Null(policy.LocalPattern);
    }

    [Fact]
    public void P2_C04_X12_flightflow_policy_owns_no_local_counter_and_activates_only_with_a_verified_delegation()
    {
        var policy = Policy(Args(InventoryAuthority.FlightFlow, providerKey: "FlightFlow"), "SEAT_SELECTION");

        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.FlightFlow, LocalInventoryPattern.FlightCount, "FlightFlow", Count())));
        BusinessAssert.Throws(16608, 409, () => policy.Activate(Evidence(PricingUnit.PerSeat), 1, Now));
        BusinessAssert.Throws(16609, 422, () => policy.Activate(Evidence(PricingUnit.PerSeat, flightFlow: InventoryReferenceCheck.NotFound), 1, Now));
        Assert.Equal(InventoryRecordStatus.Draft, policy.Status);

        policy.Activate(Evidence(PricingUnit.PerSeat, flightFlow: Ok), 1, Now);

        Assert.Equal(InventoryRecordStatus.Active, policy.Status);
        Assert.Null(policy.CountConsumption);
    }

    [Fact]
    public void P2_F02_F09_two_products_bind_the_same_physical_count_resource_instead_of_owning_a_counter_each()
    {
        var cabin = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count()), "PET_IN_CABIN", 6001);
        var small = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count()), "PET_SMALL_DOG", 6002);
        var child = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)), "MEAL_CHML", 6003);
        var vegetarian = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)), "MEAL_VGML", 6004);

        cabin.Activate(Evidence(countResource: Ok), 1, Now);
        small.Activate(Evidence(countResource: Ok, countUnitOfLiveSources: InventoryCountUnit.AnimalCarrier), 1, Now);
        child.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok), 1, Now);
        vegetarian.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok), 1, Now);

        Assert.Equal(cabin.CountConsumption, small.CountConsumption);
        Assert.Equal((PetResource, PetResource), (cabin.CountConsumption!.ResourceId, small.CountConsumption!.ResourceId));
        Assert.Equal((MealResource, MealResource), (child.CountConsumption!.ResourceId, vegetarian.CountConsumption!.ResourceId));
        Assert.NotEqual(cabin.ServiceDefinitionRef, small.ServiceDefinitionRef);
        Assert.Equal(2, Count(perUnit: 1).RequiredUnits(2));
        Assert.Equal(6, Count(perUnit: 3).RequiredUnits(2));
        BusinessAssert.Throws(16602, 422, () => Count(perUnit: int.MaxValue).RequiredUnits(2));
        BusinessAssert.Throws(16602, 422, () => Count(perUnit: 0));
        BusinessAssert.Throws(16602, 422, () => Count(resourceId: 0));
        BusinessAssert.Throws(16602, 422, () => Count(unit: (InventoryCountUnit)99));
    }

    [Fact]
    public void P2_W01_fixed_5kg_bundle_uses_5kg_in_binding_even_when_price_per_item()
    {
        var bundle = FlightWeightConsumption.FixedPerUnit(BagWeightResource, 5m);
        var policy = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, weight: bundle), "XBAG_WEIGHT_5KG");

        policy.Activate(Evidence(PricingUnit.PerItem, weightResource: Ok), 1, Now);

        Assert.Equal(
            (FlightWeightConsumptionMode.FixedKgPerAcceptedUnit, (decimal?)5m, BagWeightResource),
            (policy.WeightConsumption!.ConsumptionMode, policy.WeightConsumption.FixedKgPerUnit, policy.WeightConsumption.WeightResourceId));
        Assert.Equal(5m, bundle.RequiredKg(1, null));
        Assert.NotEqual(1m, bundle.RequiredKg(1, null));
        Assert.DoesNotContain(typeof(FlightWeightConsumption).GetMethods(), method => method.GetParameters().Any(parameter => parameter.ParameterType == typeof(string)));
    }

    [Fact]
    public void P2_W02_W03_weight_demand_is_quantity_times_fixed_kg_or_the_accepted_weight_with_three_decimals()
    {
        var tenKilo = FlightWeightConsumption.FixedPerUnit(BagWeightResource, 10m);
        var byWeight = FlightWeightConsumption.AcceptedWeight(BagWeightResource);

        Assert.Equal(20m, tenKilo.RequiredKg(2, null));
        Assert.Equal(12.345m, byWeight.RequiredKg(1, 12.345m));
        Assert.Null(byWeight.FixedKgPerUnit);
        BusinessAssert.Throws(16602, 422, () => byWeight.RequiredKg(1, null));
        BusinessAssert.Throws(16602, 422, () => byWeight.RequiredKg(1, 12.3456m));
        BusinessAssert.Throws(16602, 422, () => byWeight.RequiredKg(1, 0m));
        BusinessAssert.Throws(16602, 422, () => tenKilo.RequiredKg(0, null));
        BusinessAssert.Throws(16602, 422, () => FlightWeightConsumption.FixedPerUnit(BagWeightResource, 5.0005m));
        BusinessAssert.Throws(16602, 422, () => FlightWeightConsumption.FixedPerUnit(BagWeightResource, 0m));
        BusinessAssert.Throws(16602, 422, () => FlightWeightConsumption.FixedPerUnit(BagWeightResource, -1m));
        BusinessAssert.Throws(16602, 422, () => FlightWeightConsumption.FixedPerUnit(0, 5m));
        BusinessAssert.Throws(16602, 422, () => FlightWeightConsumption.FixedPerUnit(BagWeightResource, decimal.MaxValue).RequiredKg(2, null));
    }

    [Fact]
    public void P2_C07_a_unit_mismatch_between_pricing_and_consumption_is_refused_at_activation()
    {
        var byWeight = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, weight: FlightWeightConsumption.AcceptedWeight(BagWeightResource)), "XBAG_KILO", 6001);
        var fixedKilo = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, weight: FlightWeightConsumption.FixedPerUnit(BagWeightResource, 5m)), "XBAG_KILO", 6002);
        var countOnly = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count(unit: InventoryCountUnit.Piece)), "XBAG_KILO", 6003);
        var otherUnit = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count(unit: InventoryCountUnit.Piece)), "PET_IN_CABIN", 6004);

        BusinessAssert.Throws(16618, 409, () => byWeight.Activate(Evidence(PricingUnit.PerItem, weightResource: Ok), 1, Now));
        BusinessAssert.Throws(16618, 409, () => fixedKilo.Activate(Evidence(PricingUnit.PerKilogram, weightResource: Ok), 1, Now));
        BusinessAssert.Throws(16618, 409, () => countOnly.Activate(Evidence(PricingUnit.PerKilogram, countResource: Ok), 1, Now));
        BusinessAssert.Throws(16618, 409, () => otherUnit.Activate(Evidence(countResource: Ok, countUnitOfLiveSources: InventoryCountUnit.AnimalCarrier), 1, Now));
        BusinessAssert.Throws(16618, 409, () => otherUnit.Activate(Evidence(null, countResource: Ok), 1, Now));

        byWeight.Activate(Evidence(PricingUnit.PerKilogram, weightResource: Ok), 1, Now);

        Assert.Equal(InventoryRecordStatus.Active, byWeight.Status);
        Assert.All(new[] { fixedKilo, countOnly, otherUnit }, policy => Assert.Equal(InventoryRecordStatus.Draft, policy.Status));
    }

    [Fact]
    public void P2_W04_count_and_weight_binding_is_closed_two_resource_contract()
    {
        var bike = Policy(
            Args(
                InventoryAuthority.Local,
                LocalInventoryPattern.FlightCountPlusWeight,
                count: Count(8003, 1, InventoryCountUnit.Equipment),
                weight: FlightWeightConsumption.FixedPerUnit(BagWeightResource, 12m)),
            "SPORT_BIKE");
        var bindings = typeof(AncillaryInventoryPolicy).GetProperties().Where(property => property.Name.EndsWith("Consumption", StringComparison.Ordinal)).ToList();

        Assert.Equal(new[] { "CountConsumption", "SlotConsumption", "WeightConsumption" }, bindings.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain(bindings, property => typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType));
        Assert.Equal(3, typeof(InventoryPolicyArgs).GetProperties().Count(property => property.Name.EndsWith("Consumption", StringComparison.Ordinal)));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCountPlusWeight, count: Count(), slot: AirportSlotConsumption.Create(Lounge, 90, 1))));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, count: Count(), weight: FlightWeightConsumption.FixedPerUnit(BagWeightResource, 12m))));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, count: Count())));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, count: Count())));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCount, "Provider", Count())));

        var countOnly = Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.FlightCountPlusWeight, count: Count(8003, 1, InventoryCountUnit.Equipment)), "SPORT_SKI", 6002);

        BusinessAssert.Throws(16606, 409, () => countOnly.Activate(Evidence(PricingUnit.PerPiece, countResource: Ok, weightResource: Ok), 1, Now));
        BusinessAssert.Throws(16608, 409, () => bike.Activate(Evidence(PricingUnit.PerPiece, countResource: Ok), 1, Now));
        BusinessAssert.Throws(16609, 422, () => bike.Activate(Evidence(PricingUnit.PerPiece, countResource: InventoryReferenceCheck.NotFound, weightResource: Ok), 1, Now));

        bike.Activate(Evidence(PricingUnit.PerPiece, countResource: Ok, weightResource: Ok), 1, Now);

        Assert.Equal((InventoryRecordStatus.Active, 8003L, BagWeightResource, 12m), (bike.Status, bike.CountConsumption!.ResourceId, bike.WeightConsumption!.WeightResourceId, bike.WeightConsumption.RequiredKg(1, null)));
    }

    [Fact]
    public void P2_A07_airport_slot_binding_is_a_facility_with_occupancy_minutes_and_people_per_unit()
    {
        var binding = AirportSlotConsumption.Create(Lounge, 90, 1);
        var policy = Policy(
            Args(InventoryAuthority.Local, LocalInventoryPattern.AirportSlot, slot: binding, limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerServiceDate, 1, "LOUNGE")),
            "LOUNGE_DOTAIR_IKA");

        Assert.Equal((Lounge, 90, 1), (policy.SlotConsumption!.FacilityId, policy.SlotConsumption.OccupancyMinutes, policy.SlotConsumption.PeoplePerAcceptedUnit));
        Assert.Equal((PassengerUsageLimitScope.PerServiceDate, 1, "LOUNGE", policy.Id), policy.PassengerUsageLimits.Select(limit => (limit.LimitScope, limit.MaxUnits, limit.CountingFamilyCode, limit.InventoryPolicyId)).Single());
        BusinessAssert.Throws(16602, 422, () => AirportSlotConsumption.Create(0, 90, 1));
        BusinessAssert.Throws(16602, 422, () => AirportSlotConsumption.Create(Lounge, 0, 1));
        BusinessAssert.Throws(16602, 422, () => AirportSlotConsumption.Create(Lounge, 90, 0));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, LocalInventoryPattern.AirportSlot, count: Count())));
        BusinessAssert.Throws(16608, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, slotFacility: Ok), 1, Now));
        BusinessAssert.Throws(16608, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, families: ("LOUNGE", Ok)), 1, Now));

        policy.Activate(Evidence(PricingUnit.PerPassenger, slotFacility: Ok, families: ("LOUNGE", Ok)), 1, Now);

        Assert.Equal(InventoryRecordStatus.Active, policy.Status);
    }

    [Fact]
    public void P2_P01_passenger_usage_limit_is_a_policy_child_beside_the_physical_binding_and_never_a_counter()
    {
        var policy = Policy(
            Args(
                InventoryAuthority.Local,
                LocalInventoryPattern.FlightCount,
                count: Count(MealResource, 1, InventoryCountUnit.Item),
                limits: [new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerFlightOccurrence, 1, " meal "), new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerOrder, 2, "MEAL")]),
            "MEAL_CHML");

        Assert.Equal(
            new[] { (PassengerUsageLimitScope.PerFlightOccurrence, 1, "MEAL"), (PassengerUsageLimitScope.PerOrder, 2, "MEAL") },
            policy.PassengerUsageLimits.Select(limit => (limit.LimitScope, limit.MaxUnits, limit.CountingFamilyCode)));
        Assert.Equal(2, policy.PassengerUsageLimits.Select(limit => limit.Id).Distinct().Count());
        Assert.NotNull(policy.CountConsumption);
        Assert.DoesNotContain(
            typeof(PassengerUsageLimit).GetProperties(),
            property => property.Name is "UsedUnits" or "Consumed" or "Remaining" or "TravellerId" or "Held" or "Confirmed");

        PassengerUsageLimitArgs Limit(PassengerUsageLimitScope scope, int max, string code) => new(scope, max, code);

        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, limits: [Limit(PassengerUsageLimitScope.PerOrder, 1, "MEAL"), Limit(PassengerUsageLimitScope.PerOrder, 2, "MEAL")])));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, limits: Limit(PassengerUsageLimitScope.PerOrder, 0, "MEAL"))));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, limits: Limit((PassengerUsageLimitScope)9, 1, "MEAL"))));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, limits: Limit(PassengerUsageLimitScope.PerOrder, 1, " "))));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited, limits: Limit(PassengerUsageLimitScope.PerOrder, 1, new string('M', 31)))));
        BusinessAssert.Throws(16608, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok), 1, Now));
        BusinessAssert.Throws(16609, 422, () => policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", InventoryReferenceCheck.NotFound)), 1, Now));

        policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", Ok)), 1, Now);

        Assert.Equal(InventoryRecordStatus.Active, policy.Status);
    }

    [Fact]
    public void P2_P02_P03_P04_a_usage_scope_is_keyed_by_a_stable_traveller_identity_and_never_falls_back_to_the_order()
    {
        var policy = Policy(
            Args(
                InventoryAuthority.Unlimited,
                limits:
                [
                    new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerFlightOccurrence, 1, "MEAL"),
                    new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerOrder, 1, "MEAL"),
                    new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerServiceDate, 1, "MEAL")
                ]));
        var perFlight = policy.PassengerUsageLimits.Single(limit => limit.LimitScope == PassengerUsageLimitScope.PerFlightOccurrence);
        var perOrder = policy.PassengerUsageLimits.Single(limit => limit.LimitScope == PassengerUsageLimitScope.PerOrder);
        var perDate = policy.PassengerUsageLimits.Single(limit => limit.LimitScope == PassengerUsageLimitScope.PerServiceDate);
        var first = new PassengerUsageSubject("traveller-7", 501, 31, Flight, new DateOnly(2027, 4, 6));

        Assert.Equal(perFlight.KeyFor(first), perFlight.KeyFor(first with { OrderId = 502, OrderTravellerId = 99 }));
        Assert.NotEqual(perFlight.KeyFor(first), perFlight.KeyFor(first with { StableTravellerIdentity = "traveller-8" }));
        Assert.NotEqual(perFlight.KeyFor(first), perFlight.KeyFor(first with { FlightId = Flight + 1 }));
        Assert.Equal(perDate.KeyFor(first), perDate.KeyFor(first with { FlightId = Flight + 1, OrderId = 502 }));
        Assert.NotEqual(perDate.KeyFor(first), perDate.KeyFor(first with { ServiceDate = new DateOnly(2027, 4, 7) }));
        Assert.Equal(perOrder.KeyFor(first), perOrder.KeyFor(first with { StableTravellerIdentity = null, FlightId = Flight + 1 }));
        Assert.NotEqual(perOrder.KeyFor(first), perOrder.KeyFor(first with { OrderId = 502 }));
        Assert.NotEqual(perOrder.KeyFor(first), perOrder.KeyFor(first with { OrderTravellerId = 32 }));
        BusinessAssert.Throws(16602, 422, () => perFlight.KeyFor(first with { StableTravellerIdentity = null }));
        BusinessAssert.Throws(16602, 422, () => perFlight.KeyFor(first with { FlightId = null }));
        BusinessAssert.Throws(16602, 422, () => perDate.KeyFor(first with { StableTravellerIdentity = " " }));
        BusinessAssert.Throws(16602, 422, () => perDate.KeyFor(first with { ServiceDate = null }));
        BusinessAssert.Throws(16602, 422, () => perOrder.KeyFor(first with { OrderId = null }));
    }

    [Theory]
    [InlineData(LocalInventoryPattern.DailyCount)]
    [InlineData(LocalInventoryPattern.RoomNight)]
    [InlineData(LocalInventoryPattern.AssignedAsset)]
    public void P2_D04_H06_R03_a_local_pattern_without_a_verified_source_model_stays_draft_and_is_never_unlimited(LocalInventoryPattern pattern)
    {
        var policy = Policy(Args(InventoryAuthority.Local, pattern), "HOTEL_ROOM_STD");

        Assert.Equal((InventoryRecordStatus.Draft, pattern), (policy.Status, policy.LocalPattern!.Value));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, pattern, count: Count())));
        BusinessAssert.Throws(16607, 409, () => policy.Activate(Evidence(PricingUnit.PerRoom, countResource: Ok, weightResource: Ok, slotFacility: Ok), 1, Now));
        Assert.Equal(InventoryRecordStatus.Draft, policy.Status);
        Assert.DoesNotContain(
            typeof(AncillaryInventoryPolicy).Assembly.GetTypes(),
            type => type.Name is "DailyServiceInventory" or "RoomNightInventory" or "AssignedAssetInventory" or "AncillaryStockPool" or "InventoryResourceDefinition");
    }

    [Fact]
    public void P2_X10_a_draft_is_changed_with_its_expected_version_and_an_active_policy_only_moves_through_its_lifecycle()
    {
        var ids = new SequentialIdGenerator();
        var policy = Policy(Args(InventoryAuthority.Unlimited, limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerOrder, 1, "MEAL")));
        var limitId = policy.PassengerUsageLimits.Single().Id;

        BusinessAssert.Throws(16605, 409, () => policy.Change(Args(InventoryAuthority.Unlimited), 2, ids, Now));
        BusinessAssert.Throws(16602, 422, () => policy.Change(Args(InventoryAuthority.Unlimited, count: Count()), 1, ids, Now));

        policy.Change(
            Args(
                InventoryAuthority.Local,
                LocalInventoryPattern.FlightCount,
                count: Count(MealResource, 1, InventoryCountUnit.Item),
                limits: [new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerOrder, 2, "MEAL"), new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerFlightOccurrence, 1, "MEAL")]),
            1,
            ids,
            Now.AddMinutes(1));

        Assert.Equal((2L, InventoryAuthority.Local, Now.AddMinutes(1), Now), (policy.Version, policy.Authority, policy.UpdatedAt, policy.CreatedAt));
        Assert.Equal((limitId, 2), policy.PassengerUsageLimits.Where(limit => limit.LimitScope == PassengerUsageLimitScope.PerOrder).Select(limit => (limit.Id, limit.MaxUnits)).Single());
        BusinessAssert.Throws(16603, 409, () => policy.Suspend(2, Now));
        BusinessAssert.Throws(16605, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", Ok)), 1, Now));

        policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", Ok)), 2, Now.AddMinutes(2));

        BusinessAssert.Throws(16603, 409, () => policy.Change(Args(InventoryAuthority.Unlimited), 3, ids, Now));
        BusinessAssert.Throws(16603, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", Ok)), 3, Now));
        BusinessAssert.Throws(16605, 409, () => policy.Suspend(2, Now));

        policy.Suspend(3, Now.AddMinutes(3));

        Assert.Equal((InventoryRecordStatus.Suspended, 4L, Now.AddMinutes(3), InventoryAuthority.Local), (policy.Status, policy.Version, policy.SuspendedAt!.Value, policy.Authority));
        BusinessAssert.Throws(16608, 409, () => policy.Activate(Evidence(PricingUnit.PerPassenger, families: ("MEAL", Ok)), 4, Now));

        policy.Activate(Evidence(PricingUnit.PerPassenger, countResource: Ok, families: ("MEAL", Ok)), 4, Now.AddMinutes(4));

        Assert.Equal((InventoryRecordStatus.Active, 5L, Now.AddMinutes(2)), (policy.Status, policy.Version, policy.ActivatedAt!.Value));
        Assert.Null(policy.SuspendedAt);

        policy.Retire(5, Now.AddMinutes(5));

        Assert.Equal((InventoryRecordStatus.Retired, 6L, Now.AddMinutes(5)), (policy.Status, policy.Version, policy.RetiredAt!.Value));
        BusinessAssert.Throws(16603, 409, () => policy.Retire(6, Now));
        BusinessAssert.Throws(16603, 409, () => policy.Activate(Evidence(), 6, Now));
        BusinessAssert.Throws(16603, 409, () => policy.Change(Args(InventoryAuthority.Unlimited), 6, ids, Now));
    }

    [Fact]
    public void P2_X02_a_policy_identity_is_an_owner_airline_and_a_service_reference()
    {
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited), airline: 0));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited), reference: " "));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited), reference: new string('X', 31)));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Unlimited) with { ServiceDefinitionId = 0 }));
        BusinessAssert.Throws(16602, 422, () => Policy(Args(InventoryAuthority.Local, (LocalInventoryPattern)9)));

        var policy = Policy(Args(InventoryAuthority.Unlimited), "MEAL_CHML");

        Assert.Equal((Airline, "MEAL_CHML", Definition), (policy.OwnerAirlineId, policy.ServiceDefinitionRef, policy.ServiceDefinitionId));
        Assert.Equal(
            new[]
            {
                "ActivatedAt", "Authority", "CountConsumption", "CreatedAt", "LocalPattern", "OwnerAirlineId", "PassengerUsageLimits", "ProviderKey", "RetiredAt",
                "ServiceDefinitionId", "ServiceDefinitionRef", "SlotConsumption", "Status", "SuspendedAt", "UpdatedAt", "Version", "WeightConsumption"
            },
            P1Fixtures.PropertiesOf<AncillaryInventoryPolicy>());
    }
}
