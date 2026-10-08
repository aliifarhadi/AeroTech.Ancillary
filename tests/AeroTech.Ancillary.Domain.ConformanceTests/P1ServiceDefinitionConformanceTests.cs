using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P1ServiceDefinitionConformanceTests
{
    private static AncillaryServiceDefinition Active()
    {
        var definition = CarrierDefinition();

        definition.Activate(Supplier(), Now);

        return definition;
    }

    private static void ChangeName(AncillaryServiceDefinition definition, string commercialName)
        => definition.Change(
            definition.SupplierId,
            definition.ServiceSubCode,
            definition.SubCodeSource,
            new ServiceDefinitionClassificationArgs(
                definition.ServiceTypeCode,
                definition.GroupCode,
                definition.SubGroupCode,
                definition.Description1Code,
                definition.Description2Code),
            definition.PricingUnit!.Value,
            definition.ServiceDateBasis!.Value,
            commercialName,
            definition.Description,
            DocumentDefinition.Create(definition.Document.Type, definition.Document.Rfic, definition.Document.Rfisc),
            BookingDefinition.Create(definition.Booking.Method, definition.Booking.SsrCode, definition.Booking.SsimCode),
            definition.SalesEffectiveFrom,
            definition.SalesDiscontinueOn);

    [Fact]
    public void P1_REQ_the_definition_keeps_exactly_the_baseline_shape()
    {
        Assert.Equal(
            new[]
            {
                "ActivatedAt", "Booking", "CommercialName", "CreatedAt", "Description", "Description1Code", "Description2Code",
                "Document", "GroupCode", "OwnerAirlineId", "PricingUnit", "RetiredAt", "SalesDiscontinueOn", "SalesEffectiveFrom",
                "ServiceDateBasis", "ServiceDefinitionRef", "ServiceSubCode", "ServiceTypeCode", "Status", "SubCodeSource", "SubGroupCode",
                "SupplierId", "SuspendedAt", "Version"
            },
            PropertiesOf<AncillaryServiceDefinition>());
        Assert.Equal(new[] { "Rfic", "Rfisc", "Type" }, PropertiesOf<DocumentDefinition>());
        Assert.Equal(new[] { "Method", "SsimCode", "SsrCode" }, PropertiesOf<BookingDefinition>());
    }

    [Fact]
    public void P1_C01_an_industry_sub_code_must_exist_in_the_read_only_reference()
    {
        Assert.Equal(new[] { "0BX", "0CC" }, IndustryServiceSubCodeReference.Entries.Select(entry => entry.Code).OrderBy(code => code, StringComparer.Ordinal));

        BusinessAssert.Throws(16207, 422, () => AncillaryServiceDefinition.Define(
            1002,
            Airline,
            2001,
            "WHEELCHAIR_RAMP",
            1,
            "0ZZ",
            ServiceSubCodeSource.Industry,
            new ServiceDefinitionClassificationArgs(null, null, null, null, null),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Wheelchair",
            null,
            DocumentDefinition.Create(AncillaryDocumentType.None, null, null),
            BookingDefinition.Create(BookingMethod.Ssr, "WCHR", null),
            null,
            null,
            Now));
    }

    [Fact]
    public void P1_C02_a_carrier_defined_service_authors_its_own_classification()
    {
        var definition = CarrierDefinition();

        Assert.Equal(ServiceSubCodeSource.CarrierDefined, definition.SubCodeSource);
        Assert.Equal(("F", "MVG", "ML", "VG"), (definition.ServiceTypeCode, definition.ServiceSubCode, definition.GroupCode, definition.SubGroupCode));
        Assert.Null(IndustryServiceSubCodeReference.Find("MVG"));
    }

    [Fact]
    public void P1_C03_activation_and_reactivation_require_an_active_supplier()
    {
        var retiredSupplier = Supplier();

        retiredSupplier.Retire(Now);

        var draft = CarrierDefinition();

        BusinessAssert.Throws(16206, 422, () => draft.Activate(retiredSupplier, Now));
        Assert.Equal(ServiceDefinitionStatus.Draft, draft.Status);

        draft.Activate(Supplier(), Now);
        draft.Suspend(Now);

        BusinessAssert.Throws(16206, 422, () => draft.Reactivate(retiredSupplier));
        BusinessAssert.Throws(16202, 422, () => draft.Reactivate(Supplier(2999)));
        Assert.Equal(ServiceDefinitionStatus.Suspended, draft.Status);
    }

    [Fact]
    public void P1_C04_a_draft_can_be_edited()
    {
        var definition = CarrierDefinition();

        definition.Change(
            2002,
            "MCH",
            ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", "ML", "CH", "KD", null),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Child meal",
            null,
            DocumentDefinition.Create(AncillaryDocumentType.None, null, null),
            BookingDefinition.Create(BookingMethod.Ssr, "CHML", null),
            null,
            new DateOnly(2027, 6, 30));

        Assert.Equal(ServiceDefinitionStatus.Draft, definition.Status);
        Assert.Equal((2002L, "MCH", "CH", "KD"), (definition.SupplierId, definition.ServiceSubCode, definition.SubGroupCode, definition.Description1Code));
        Assert.Equal(("Child meal", (string?)null), (definition.CommercialName, definition.Description));
        Assert.Equal((AncillaryDocumentType.None, (string?)null), (definition.Document.Type, definition.Document.Rfisc));
        Assert.Equal("CHML", definition.Booking.SsrCode);
        Assert.Equal(((DateOnly?)null, new DateOnly(2027, 6, 30)), (definition.SalesEffectiveFrom, definition.SalesDiscontinueOn!.Value));
        Assert.Equal(("MEAL_VGML", 1, Airline), (definition.ServiceDefinitionRef, definition.Version, definition.OwnerAirlineId));
    }

    [Fact]
    public void P1_C04_an_edit_is_validated_like_a_definition_and_a_refused_edit_changes_nothing()
    {
        var definition = CarrierDefinition();

        BusinessAssert.Throws(16207, 422, () => definition.Change(
            2001,
            "0ZZ",
            ServiceSubCodeSource.Industry,
            new ServiceDefinitionClassificationArgs(null, null, null, null, null),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Renamed",
            null,
            DocumentDefinition.Create(AncillaryDocumentType.None, null, null),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null,
            null));
        BusinessAssert.Throws(16202, 422, () => ChangeName(definition, ""));

        Assert.Equal(("MVG", "Vegetarian meal", "ML"), (definition.ServiceSubCode, definition.CommercialName, definition.GroupCode));
    }

    [Theory]
    [InlineData(ServiceDefinitionStatus.Active)]
    [InlineData(ServiceDefinitionStatus.Suspended)]
    [InlineData(ServiceDefinitionStatus.Retired)]
    public void P1_C04_a_definition_that_left_draft_rejects_semantic_mutation(ServiceDefinitionStatus status)
    {
        var definition = Active();

        if (status == ServiceDefinitionStatus.Suspended)
            definition.Suspend(Now);

        if (status == ServiceDefinitionStatus.Retired)
            definition.Retire(Now);

        BusinessAssert.Throws(16203, 409, () => ChangeName(definition, "Renamed"));

        Assert.Equal("Vegetarian meal", definition.CommercialName);
        Assert.Equal(status, definition.Status);
    }

    [Fact]
    public void P1_C05_K06_the_lifecycle_follows_the_existing_status_values_only()
    {
        Assert.Equal(
            new[] { "Active", "Draft", "Retired", "Suspended" },
            Enum.GetNames<ServiceDefinitionStatus>().OrderBy(name => name, StringComparer.Ordinal));

        var definition = CarrierDefinition();

        BusinessAssert.Throws(16203, 409, () => definition.Suspend(Now));
        BusinessAssert.Throws(16203, 409, () => definition.Reactivate(Supplier()));

        definition.Activate(Supplier(), Now.AddMinutes(1));

        Assert.Equal((ServiceDefinitionStatus.Active, Now.AddMinutes(1)), (definition.Status, definition.ActivatedAt!.Value));
        BusinessAssert.Throws(16203, 409, () => definition.Activate(Supplier(), Now));
        BusinessAssert.Throws(16203, 409, () => definition.Reactivate(Supplier()));

        definition.Suspend(Now.AddMinutes(2));

        Assert.Equal((ServiceDefinitionStatus.Suspended, Now.AddMinutes(2)), (definition.Status, definition.SuspendedAt!.Value));
        BusinessAssert.Throws(16203, 409, () => definition.Suspend(Now));
        BusinessAssert.Throws(16203, 409, () => definition.Activate(Supplier(), Now));

        definition.Reactivate(Supplier());

        Assert.Equal(ServiceDefinitionStatus.Active, definition.Status);
        Assert.Null(definition.SuspendedAt);

        definition.Retire(Now.AddMinutes(3));

        Assert.Equal((ServiceDefinitionStatus.Retired, Now.AddMinutes(3)), (definition.Status, definition.RetiredAt!.Value));
        BusinessAssert.Throws(16203, 409, () => definition.Retire(Now));
        BusinessAssert.Throws(16203, 409, () => definition.Reactivate(Supplier()));
        BusinessAssert.Throws(16203, 409, () => definition.Suspend(Now));
        BusinessAssert.Throws(16203, 409, () => definition.Activate(Supplier(), Now));
    }

    [Fact]
    public void P1_C05_a_draft_and_a_suspended_definition_can_be_retired_directly()
    {
        var draft = CarrierDefinition(1001);
        var suspended = CarrierDefinition(1002);

        suspended.Activate(Supplier(), Now);
        suspended.Suspend(Now);
        draft.Retire(Now);
        suspended.Retire(Now);

        Assert.Equal(ServiceDefinitionStatus.Retired, draft.Status);
        Assert.Equal(ServiceDefinitionStatus.Retired, suspended.Status);
    }

    [Fact]
    public void P1_C06_a_revision_is_a_new_draft_with_a_new_id_and_the_next_version()
    {
        var original = Active();

        var revision = original.Revise(1500, 2, Now.AddDays(1));

        Assert.Equal((1500L, 2, ServiceDefinitionStatus.Draft), (revision.Id, revision.Version, revision.Status));
        Assert.Equal(Now.AddDays(1), revision.CreatedAt);
        Assert.Null(revision.ActivatedAt);
        Assert.Equal(
            (original.OwnerAirlineId, original.SupplierId, original.ServiceDefinitionRef, original.ServiceTypeCode, original.ServiceSubCode),
            (revision.OwnerAirlineId, revision.SupplierId, revision.ServiceDefinitionRef, revision.ServiceTypeCode, revision.ServiceSubCode));
        Assert.Equal(
            (original.SubCodeSource, original.GroupCode, original.SubGroupCode, original.Description1Code, original.Description2Code),
            (revision.SubCodeSource, revision.GroupCode, revision.SubGroupCode, revision.Description1Code, revision.Description2Code));
        Assert.Equal(
            (original.CommercialName, original.Description, original.SalesEffectiveFrom, original.SalesDiscontinueOn),
            (revision.CommercialName, revision.Description, revision.SalesEffectiveFrom, revision.SalesDiscontinueOn));
        Assert.Equal(original.Document, revision.Document);
        Assert.Equal(original.Booking, revision.Booking);
        Assert.NotSame(original.Document, revision.Document);
        Assert.NotSame(original.Booking, revision.Booking);
        Assert.Equal((1001L, 1, ServiceDefinitionStatus.Active), (original.Id, original.Version, original.Status));

        ChangeName(revision, "Vegetarian meal (new recipe)");

        Assert.Equal("Vegetarian meal", original.CommercialName);
        Assert.Equal("Vegetarian meal (new recipe)", revision.CommercialName);
    }

    [Fact]
    public void P1_C06_only_a_published_definition_can_be_revised_and_the_version_must_grow()
    {
        var draft = CarrierDefinition();

        BusinessAssert.Throws(16203, 409, () => draft.Revise(1500, 2, Now));

        var active = Active();

        BusinessAssert.Throws(16202, 422, () => active.Revise(1500, 1, Now));

        active.Suspend(Now);

        Assert.Equal(2, active.Revise(1500, 2, Now).Version);

        active.Retire(Now);

        BusinessAssert.Throws(16203, 409, () => active.Revise(1501, 3, Now));
    }

    [Fact]
    public void P1_C07_booking_and_document_definitions_hold_every_authored_value()
    {
        var definition = AncillaryServiceDefinition.Define(
            1003,
            Airline,
            2001,
            "PET_CABIN",
            1,
            "PTC",
            ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", "PT", null, null, null),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Pet in cabin",
            null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "G", "PTC"),
            BookingDefinition.Create(BookingMethod.Ssr, "PETC", "PETC"),
            null,
            null,
            Now);

        Assert.Equal((AncillaryDocumentType.EmdAssociated, "G", "PTC"), (definition.Document.Type, definition.Document.Rfic, definition.Document.Rfisc));
        Assert.Equal((BookingMethod.Ssr, "PETC", "PETC"), (definition.Booking.Method, definition.Booking.SsrCode, definition.Booking.SsimCode));
        Assert.Equal(
            new[]
            {
                "AuxiliarySegment", "DisplayPriceContactCarrierForBooking", "NoBookingProcessRequired", "PerServiceRecord", "Ssr"
            },
            Enum.GetNames<BookingMethod>().OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            new[] { "EmdAssociated", "EmdStandalone", "None" },
            Enum.GetNames<AncillaryDocumentType>().OrderBy(name => name, StringComparer.Ordinal));
    }
}
