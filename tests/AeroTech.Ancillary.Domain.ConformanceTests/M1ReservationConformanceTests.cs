using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class M1ReservationConformanceTests
{
    private static AncillaryReservation Hold(
        DateTimeOffset? requestedExpiresAt = null,
        params AncillaryReservationUnitArgs[] units)
        => AncillaryReservation.Hold(
            8001,
            "IDEMP-1",
            7001,
            "ORD-7001",
            requestedExpiresAt,
            units.Length == 0 ? [Unit()] : units,
            new SequentialIdGenerator(),
            Now);

    [Fact]
    public void M1_N01_N02_N03_a_hold_stores_the_fulfillment_facts_by_id()
    {
        var reservation = Hold(Now.AddMinutes(30));

        Assert.Equal("IDEMP-1", reservation.IdempotencyKey);
        Assert.Equal(7001, reservation.OrderId);
        Assert.Equal("ORD-7001", reservation.Reference);
        Assert.Equal(Now.AddMinutes(30), reservation.RequestedExpiresAt);
        Assert.Equal(Now.AddMinutes(30), reservation.ExpiresAt);
        Assert.Equal(AncillaryReservationStatus.Held, reservation.Status);

        var unit = Assert.Single(reservation.Units);
        Assert.Equal(9001, unit.OrderServiceId);
        Assert.Equal(1001, unit.ServiceDefinitionId);
        Assert.Equal(501, unit.ProvisionId);
        Assert.Equal(71, unit.TravellerId);
        Assert.Equal(ServiceCoverageScope.Sector, unit.CoverageScope);
        Assert.Equal(new long[] { 301 }, unit.CoveredFlightIds);
        Assert.Equal(1, unit.Quantity);
        Assert.Equal(AncillaryReservationUnitStatus.Held, unit.Status);
    }

    [Fact]
    public void M1_N05_the_provision_must_belong_to_the_definition_and_both_must_exist()
    {
        var definition = LoungeDefinition();
        var otherDefinitionProvision = LoungeProvision(serviceDefinitionId: 1009);
        var supplier = LocalSupplier();

        BusinessAssert.Throws(16404, 422, () => AncillaryHoldChecks.Accept(Unit(), null, LoungeProvision(), supplier));
        BusinessAssert.Throws(16405, 422, () => AncillaryHoldChecks.Accept(Unit(), definition, null, supplier));
        BusinessAssert.Throws(16405, 422, () => AncillaryHoldChecks.Accept(Unit(), definition, otherDefinitionProvision, supplier));
    }

    [Fact]
    public void M1_N04_A12_the_supplier_comes_from_the_definition_and_routes_by_kind_only()
    {
        var definition = LoungeDefinition();
        var provision = LoungeProvision();

        BusinessAssert.Throws(16101, 404, () => AncillaryHoldChecks.Accept(Unit(), definition, provision, null));
        BusinessAssert.Throws(16101, 404, () => AncillaryHoldChecks.Accept(Unit(), definition, provision, LocalSupplier(id: 2999)));

        AncillaryHoldChecks.Accept(Unit(), definition, provision, LocalSupplier());

        var otherLocalId = LoungeDefinition(id: 1002, supplierId: 2777, reference: "LNG_IKA_2");

        AncillaryHoldChecks.Accept(Unit(serviceDefinitionId: 1002), otherLocalId, LoungeProvision(serviceDefinitionId: 1002), LocalSupplier(id: 2777));
    }

    [Fact]
    public void M1_A14_an_external_supplier_without_a_registered_adapter_fails_deterministically()
    {
        var definition = LoungeDefinition(supplierId: 2002);
        var provision = LoungeProvision();

        BusinessAssert.Throws(16104, 422, () => AncillaryHoldChecks.Accept(Unit(), definition, provision, ExternalSupplier()));
    }

    [Fact]
    public void M1_N06_the_quantity_must_fit_the_provision_rule()
    {
        var definition = LoungeDefinition();
        var supplier = LocalSupplier();

        BusinessAssert.Throws(16409, 422, () => AncillaryHoldChecks.Accept(Unit(quantity: 2), definition, LoungeProvision(), supplier));
        BusinessAssert.Throws(16409, 422, () => AncillaryHoldChecks.Accept(Unit(quantity: 1), definition, LoungeProvision(minQuantity: 2, maxQuantity: 3), supplier));
    }

    [Fact]
    public void M1_N06_the_coverage_identity_must_match_the_provision()
    {
        var definition = LoungeDefinition();
        var supplier = LocalSupplier();

        BusinessAssert.Throws(16410, 422, () => AncillaryHoldChecks.Accept(
            Unit(coverageScope: ServiceCoverageScope.Journey), definition, LoungeProvision(), supplier));

        BusinessAssert.Throws(16402, 422, () => Hold(null, Unit(coveredFlightIds: [301, 302])));
        BusinessAssert.Throws(16402, 422, () => Hold(null, Unit(coveredFlightIds: [])));
        BusinessAssert.Throws(16402, 422, () => Hold(null, Unit(travellerId: null)));
        BusinessAssert.Throws(16402, 422, () => Hold(null, Unit(coverageScope: ServiceCoverageScope.Order, travellerId: 71, coveredFlightIds: [])));
    }

    [Fact]
    public void M1_N01_the_hold_shape_is_validated()
    {
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "", 7001, "ORD", null, [Unit()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "IDEMP", 0, "ORD", null, [Unit()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "IDEMP", 7001, "ORD", Now, [Unit()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "IDEMP", 7001, "ORD", null, [], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "IDEMP", 7001, "ORD", null, [Unit(), Unit()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16402, 422, () => AncillaryReservation.Hold(
            8001, "IDEMP", 7001, "ORD", null,
            [Unit(), Unit(orderServiceId: 9002)],
            new SequentialIdGenerator(), Now));
    }

    [Fact]
    public void M1_N09_a_replay_with_the_same_content_passes_and_a_different_body_is_refused()
    {
        var reservation = Hold(Now.AddMinutes(30));

        reservation.EnsureSameContent(7001, "ORD-7001", Now.AddMinutes(30), [Unit()]);

        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", Now.AddMinutes(30), [Unit(quantity: 1, coveredFlightIds: [302])]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7002, "ORD-7001", Now.AddMinutes(30), [Unit()]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", null, [Unit()]));
    }

    [Fact]
    public void M1_N11_confirm_moves_all_units_and_replays_safely()
    {
        var reservation = Hold(Now.AddMinutes(30));

        Assert.True(reservation.Confirm(Now.AddMinutes(1)));
        Assert.Equal(AncillaryReservationStatus.Confirmed, reservation.Status);
        Assert.All(reservation.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Confirmed, unit.Status));
        Assert.Equal(Now.AddMinutes(1), reservation.UpdatedAt);

        Assert.False(reservation.Confirm(Now.AddMinutes(2)));
    }

    [Fact]
    public void M1_N11_an_expired_hold_cannot_be_confirmed_and_reads_as_expired()
    {
        var reservation = Hold(Now.AddMinutes(30));

        Assert.Equal(AncillaryReservationStatus.Expired, reservation.EffectiveStatus(Now.AddMinutes(31)));
        Assert.Equal(AncillaryReservationUnitStatus.Expired, reservation.StatusOf(reservation.Units.First(), Now.AddMinutes(30)));

        BusinessAssert.Throws(16407, 409, () => reservation.Confirm(Now.AddMinutes(31)));

        var open = Hold(null);

        Assert.Equal(AncillaryReservationStatus.Held, open.EffectiveStatus(Now.AddYears(1)));
    }

    [Fact]
    public void M1_I06_I07_I08_I09_the_reservation_carries_no_money_or_currency_members()
    {
        var names = typeof(AncillaryReservation).GetProperties().Select(property => property.Name)
            .Concat(typeof(AncillaryReservationUnit).GetProperties().Select(property => property.Name))
            .ToList();

        Assert.DoesNotContain(names, name => name.Contains("Currency", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.Contains("Revenue", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.Contains("Amount", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, name => name.Contains("Price", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M1_C08_M05_a_reservation_unit_identifies_the_service_by_id_only()
    {
        var names = typeof(AncillaryReservationUnit).GetProperties().Select(property => property.Name).ToList();

        Assert.Contains("ServiceDefinitionId", names);
        Assert.Contains("ProvisionId", names);
        Assert.DoesNotContain(names, name => name.Contains("Ref", StringComparison.Ordinal) && name != "ProviderUnitRef");
        Assert.DoesNotContain("ServiceDefinitionVersion", names);
    }

    [Fact]
    public void M1_L06_the_ancillary_domain_hosts_no_offer_evaluator()
    {
        var types = typeof(AncillaryReservation).Assembly.GetTypes().Select(type => type.Name).ToList();

        Assert.DoesNotContain(types, name => name.Contains("Evaluator", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(types, name => name.Contains("Quote", StringComparison.OrdinalIgnoreCase));
    }
}
