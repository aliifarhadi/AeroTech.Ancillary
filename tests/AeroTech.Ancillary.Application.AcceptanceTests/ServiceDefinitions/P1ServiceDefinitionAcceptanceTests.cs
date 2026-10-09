using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceDefinitions;

[Collection(DatabaseCollection.Name)]
public class P1ServiceDefinitionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P1ServiceDefinitionAcceptanceTests(TestDatabase database) => _database = database;

    private static TestDefineServiceDefinitionCommand Meal(int airlineId, long supplierId, string reference = "MEAL_VGML")
        => P1Commands.CarrierDefinition(
            airlineId,
            supplierId,
            reference,
            "MVG",
            "F",
            "ML",
            "Vegetarian meal",
            P1Commands.Ssr("VGML"),
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "G", "MVG"),
            "VG",
            "Pre-ordered vegetarian meal",
            new DateOnly(2026, 10, 1),
            new DateOnly(2027, 3, 31));

    [Fact]
    public async Task P1_C04_C07_a_draft_definition_is_edited_and_every_field_round_trips()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var caterer = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));
        var airline = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var draft = await scope.DefineServiceDefinition.DefineAsync(Meal(airlineId, caterer.Id));

        await using var editor = new AncillaryScope(_database, _clock);
        var edited = await editor.ChangeServiceDefinition.ChangeAsync(new TestChangeServiceDefinitionCommand(
            draft.Id,
            airline.Id,
            "MCH",
            ServiceSubCodeSource.CarrierDefined,
            "F",
            "ML",
            "CH",
            "KD",
            "HT",
            "Child meal",
            null,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null),
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "CHML", "CHML"),
            null,
            new DateOnly(2027, 6, 30)));

        Assert.Equal((draft.Id, 1, ServiceDefinitionStatus.Draft, "MCH"), (edited.Id, edited.Version, edited.Status, edited.ServiceSubCode));

        await using var reader = new AncillaryScope(_database, _clock);
        var aggregate = (await reader.Definitions.GetAsync(draft.Id))!;
        var detail = await reader.GetServiceDefinitionById.ExecuteAsync(draft.Id);

        Assert.Equal((airline.Id, "MCH", "CH", "KD", "HT"), (aggregate.SupplierId, aggregate.ServiceSubCode, aggregate.SubGroupCode, aggregate.Description1Code, aggregate.Description2Code));
        Assert.Equal((AncillaryDocumentType.None, (string?)null, (string?)null), (aggregate.Document.Type, aggregate.Document.Rfic, aggregate.Document.Rfisc));
        Assert.Equal((BookingMethod.Ssr, "CHML", "CHML"), (aggregate.Booking.Method, aggregate.Booking.SsrCode, aggregate.Booking.SsimCode));
        Assert.Equal((airline.Id, "Dot Air", "MEAL_VGML", 1), (detail.SupplierId, detail.SupplierName, detail.ServiceDefinitionRef, detail.Version));
        Assert.Equal(("F", "MCH", "CarrierDefined", "ML", "CH", "KD", "HT"), (detail.ServiceTypeCode, detail.ServiceSubCode, detail.SubCodeSource.Name, detail.GroupCode, detail.SubGroupCode, detail.Description1Code, detail.Description2Code));
        Assert.Equal(("Child meal", (string?)null), (detail.CommercialName, detail.Description));
        Assert.Equal(("None", (string?)null, (string?)null), (detail.DocumentType.Name, detail.DocumentRfic, detail.DocumentRfisc));
        Assert.Equal(("Ssr", "CHML", "CHML"), (detail.BookingMethod.Name, detail.BookingSsrCode, detail.BookingSsimCode));
        Assert.Equal(((DateOnly?)null, new DateOnly(2027, 6, 30)), (detail.SalesEffectiveFrom, detail.SalesDiscontinueOn!.Value));
        Assert.Equal("Draft", detail.Status.Name);

        var listed = await reader.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, Search = "Child" });

        Assert.Equal(("Child meal", "MCH", "Dot Air"), (listed.Results.Single().CommercialName, listed.Results.Single().ServiceSubCode, listed.Results.Single().SupplierName));
    }

    [Fact]
    public async Task P1_C04_an_edit_is_refused_once_the_definition_left_draft_or_when_it_is_malformed()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = Meal(airlineId, supplier.Id);
        var draft = await scope.DefineServiceDefinition.DefineAsync(definition);

        await BusinessAssert.ThrowsAsync(16207, 422, () => scope.ChangeServiceDefinition.ChangeAsync(
            P1Commands.Change(draft.Id, definition with { ServiceSubCode = "0ZZ", SubCodeSource = ServiceSubCodeSource.Industry })));
        await BusinessAssert.ThrowsAsync(16205, 422, () => scope.ChangeServiceDefinition.ChangeAsync(
            P1Commands.Change(draft.Id, definition with { SupplierId = 999_999_999 })));
        await BusinessAssert.ThrowsAsync(16201, 404, () => scope.ChangeServiceDefinition.ChangeAsync(
            P1Commands.Change(999_999_999, definition)));

        await using var activator = new AncillaryScope(_database, _clock);
        await activator.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(draft.Id));

        await using var editor = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16203, 409, () => editor.ChangeServiceDefinition.ChangeAsync(
            P1Commands.Change(draft.Id, definition with { CommercialName = "Renamed" })));

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetServiceDefinitionById.ExecuteAsync(draft.Id);

        Assert.Equal(("Vegetarian meal", "MVG", "Active"), (detail.CommercialName, detail.ServiceSubCode, detail.Status.Name));
    }

    [Fact]
    public async Task P1_C05_K06_the_lifecycle_moves_a_definition_through_the_existing_states()
    {
        var airlineId = _database.NextAirlineId();
        var start = _clock.Now;
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var draft = await scope.DefineServiceDefinition.DefineAsync(Meal(airlineId, supplier.Id));
        var command = new TestServiceDefinitionLifecycleCommand(draft.Id);

        await BusinessAssert.ThrowsAsync(16203, 409, () => scope.SuspendServiceDefinition.SuspendAsync(command));
        await BusinessAssert.ThrowsAsync(16203, 409, () => scope.ReactivateServiceDefinition.ReactivateAsync(command));

        async Task<(string Status, DateTimeOffset? ActivatedAt, DateTimeOffset? SuspendedAt, DateTimeOffset? RetiredAt)> ReadAsync()
        {
            await using var reader = new AncillaryScope(_database, _clock);
            var detail = await reader.GetServiceDefinitionById.ExecuteAsync(draft.Id);
            var row = await reader.Query.AncillaryServiceDefinitions.AsNoTracking().SingleAsync(definition => definition.Id == draft.Id);

            Assert.Equal(detail.Status.Name, row.Status.ToString());
            Assert.Equal(detail.Status.Name, (await reader.Definitions.GetAsync(draft.Id))!.Status.ToString());

            return (detail.Status.Name, detail.ActivatedAt, detail.SuspendedAt, detail.RetiredAt);
        }

        async Task<ServiceDefinitionStatus> ActAsync(Func<AncillaryScope, Task<ServiceDefinitionResult>> act)
        {
            _clock.Now = _clock.Now.AddHours(1);
            await using var writer = new AncillaryScope(_database, _clock);

            return (await act(writer)).Status;
        }

        Assert.Equal(ServiceDefinitionStatus.Active, await ActAsync(writer => writer.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(draft.Id))));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ServiceDefinitionStatus.Suspended, await ActAsync(writer => writer.SuspendServiceDefinition.SuspendAsync(command)));
        Assert.Equal(("Suspended", start.AddHours(1), start.AddHours(2), null), await ReadAsync());

        Assert.Equal(ServiceDefinitionStatus.Active, await ActAsync(writer => writer.ReactivateServiceDefinition.ReactivateAsync(command)));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ServiceDefinitionStatus.Retired, await ActAsync(writer => writer.RetireServiceDefinition.RetireAsync(command)));
        Assert.Equal(("Retired", start.AddHours(1), null, start.AddHours(4)), await ReadAsync());

        await using var late = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16203, 409, () => late.RetireServiceDefinition.RetireAsync(command));
        await BusinessAssert.ThrowsAsync(16203, 409, () => late.ReactivateServiceDefinition.ReactivateAsync(command));
        await BusinessAssert.ThrowsAsync(16203, 409, () => late.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(draft.Id)));
        await BusinessAssert.ThrowsAsync(16201, 404, () => late.SuspendServiceDefinition.SuspendAsync(new TestServiceDefinitionLifecycleCommand(999_999_999)));
    }

    [Fact]
    public async Task P1_C06_revise_creates_the_next_version_as_a_new_draft_and_keeps_the_published_version()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = Meal(airlineId, supplier.Id);
        var first = await scope.DefineServiceDefinition.DefineAsync(definition);

        await BusinessAssert.ThrowsAsync(16203, 409, () => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id)));

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(first.Id));

        await using var reviser = new AncillaryScope(_database, _clock);
        var second = await reviser.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal((2, ServiceDefinitionStatus.Draft, "MEAL_VGML", "MVG"), (second.Version, second.Status, second.ServiceDefinitionRef, second.ServiceSubCode));

        await using var editor = new AncillaryScope(_database, _clock);
        await editor.ChangeServiceDefinition.ChangeAsync(P1Commands.Change(
            second.Id,
            definition with { CommercialName = "Vegetarian meal (new recipe)", SalesDiscontinueOn = new DateOnly(2027, 12, 31) }));

        await using var reader = new AncillaryScope(_database, _clock);
        var published = await reader.GetServiceDefinitionById.ExecuteAsync(first.Id);
        var revision = await reader.GetServiceDefinitionById.ExecuteAsync(second.Id);

        Assert.Equal((1, "Active", "Vegetarian meal", new DateOnly(2027, 3, 31)), (published.Version, published.Status.Name, published.CommercialName, published.SalesDiscontinueOn!.Value));
        Assert.Equal((2, "Draft", "Vegetarian meal (new recipe)", new DateOnly(2027, 12, 31)), (revision.Version, revision.Status.Name, revision.CommercialName, revision.SalesDiscontinueOn!.Value));
        Assert.Equal(
            (published.SupplierId, published.ServiceTypeCode, published.GroupCode, published.SubGroupCode, published.DocumentRfisc, published.BookingSsrCode, published.Description),
            (revision.SupplierId, revision.ServiceTypeCode, revision.GroupCode, revision.SubGroupCode, revision.DocumentRfisc, revision.BookingSsrCode, revision.Description));
        Assert.Null(revision.ActivatedAt);

        await BusinessAssert.ThrowsAsync(16204, 409, () => reader.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(second.Id)));

        await using var publisher = new AncillaryScope(_database, _clock);
        await publisher.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(first.Id));
        await publisher.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(second.Id));
        var third = await publisher.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(second.Id));

        Assert.Equal(3, third.Version);

        await BusinessAssert.ThrowsAsync(16203, 409, () => publisher.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id)));

        await using var lister = new AncillaryScope(_database, _clock);
        var versions = await lister.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, ServiceDefinitionRef = "MEAL_VGML" });

        Assert.Equal(new[] { third.Id.ToString(), second.Id.ToString(), first.Id.ToString() }, versions.Results.Select(row => row.Id));
        Assert.Equal(new[] { 3, 2, 1 }, versions.Results.Select(row => row.Version));
        Assert.Equal(new[] { "Draft", "Active", "Retired" }, versions.Results.Select(row => row.Status.Name));
    }

    [Fact]
    public async Task P1_C05_a_suspended_version_cannot_be_reactivated_while_another_version_is_active()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var first = await scope.DefineServiceDefinition.DefineAsync(Meal(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(first.Id));
        await scope.SuspendServiceDefinition.SuspendAsync(new TestServiceDefinitionLifecycleCommand(first.Id));
        var second = await scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(second.Id));

        await using var writer = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16204, 409, () => writer.ReactivateServiceDefinition.ReactivateAsync(
            new TestServiceDefinitionLifecycleCommand(first.Id)));

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal("Suspended", (await reader.GetServiceDefinitionById.ExecuteAsync(first.Id)).Status.Name);
        Assert.Equal("Active", (await reader.GetServiceDefinitionById.ExecuteAsync(second.Id)).Status.Name);
    }

    [Fact]
    public async Task P1_K02_definitions_are_filtered_by_status_supplier_sub_code_group_and_name()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var caterer = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));
        var airline = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var meal = await scope.DefineServiceDefinition.DefineAsync(Meal(airlineId, caterer.Id));
        var lounge = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, airline.Id));
        var wifi = await scope.DefineServiceDefinition.DefineAsync(
            P1Commands.CarrierDefinition(airlineId, airline.Id, "WIFI_FULL", "WFF", "F", "WF", "Full flight Wi-Fi"));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(meal.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(lounge.Id));
        await scope.SuspendServiceDefinition.SuspendAsync(new TestServiceDefinitionLifecycleCommand(lounge.Id));

        async Task<string[]> IdsAsync(BackofficeGetAncillaryServiceDefinitionsPaginatedQuery query)
        {
            query.OwnerAirlineId = airlineId;

            return (await scope.GetServiceDefinitionsPaginated.ExecuteAsync(query)).Results.Select(row => row.Id).ToArray();
        }

        Assert.Equal(new[] { meal.Id.ToString() }, await IdsAsync(new() { Status = ServiceDefinitionStatus.Active }));
        Assert.Equal(new[] { lounge.Id.ToString() }, await IdsAsync(new() { Status = ServiceDefinitionStatus.Suspended }));
        Assert.Equal(new[] { wifi.Id.ToString() }, await IdsAsync(new() { Status = ServiceDefinitionStatus.Draft }));
        Assert.Equal(new[] { meal.Id.ToString() }, await IdsAsync(new() { SupplierId = caterer.Id }));
        Assert.Equal(new[] { lounge.Id.ToString() }, await IdsAsync(new() { ServiceSubCode = "0BX" }));
        Assert.Equal(new[] { wifi.Id.ToString() }, await IdsAsync(new() { GroupCode = "WF" }));
        Assert.Equal(new[] { wifi.Id.ToString() }, await IdsAsync(new() { Search = "Wi-Fi" }));
        Assert.Equal(3, (await IdsAsync(new())).Length);
        Assert.Equal(2, (await IdsAsync(new() { PageSize = 2 })).Length);
    }

    [Fact]
    public void P1_REQ_the_backoffice_edit_validator_checks_the_shape_of_the_definition_inputs()
    {
        var validator = new BackofficeChangeAncillaryServiceDefinitionCommandValidator();

        BackofficeChangeAncillaryServiceDefinitionCommand Command(
            long serviceDefinitionId = 1,
            string serviceSubCode = "MVG",
            ServiceSubCodeSource subCodeSource = ServiceSubCodeSource.CarrierDefined,
            string commercialName = "Vegetarian meal",
            AncillaryDocumentType documentType = AncillaryDocumentType.None,
            BookingMethod bookingMethod = BookingMethod.Ssr,
            string? ssrCode = "VGML")
            => new(
                serviceDefinitionId,
                2001,
                serviceSubCode,
                subCodeSource,
                "F",
                "ML",
                null,
                null,
                null,
                PricingUnit.PerPassenger,
                ServiceDateBasis.FlightDeparture,
                AncillaryProfile.Priority,
                AncillaryVariant.PriorityBoardingCheckin,
                V122Catalog.RoutingOf(documentType),
                V122Catalog.Specification(AncillaryVariant.PriorityBoardingCheckin),
                commercialName,
                null,
                new ServiceDefinitionDocumentInput(documentType, null, null),
                new ServiceDefinitionBookingInput(bookingMethod, ssrCode, null),
                null,
                null);

        string[] Errors(BackofficeChangeAncillaryServiceDefinitionCommand command)
            => validator.Validate(command).Errors.Select(error => error.PropertyName).ToArray();

        Assert.Empty(Errors(Command()));
        Assert.Contains("ServiceDefinitionId", Errors(Command(serviceDefinitionId: 0)));
        Assert.Contains("ServiceSubCode", Errors(Command(serviceSubCode: "MVGX")));
        Assert.Contains("SubCodeSource", Errors(Command(subCodeSource: (ServiceSubCodeSource)9)));
        Assert.Contains("CommercialName", Errors(Command(commercialName: "")));
        Assert.Contains("Document.Type", Errors(Command(documentType: (AncillaryDocumentType)9)));
        Assert.Contains("Booking.Method", Errors(Command(bookingMethod: (BookingMethod)9)));
        Assert.Contains("Booking.SsrCode", Errors(Command(ssrCode: "VGMLX")));
        Assert.Contains("ServiceDateBasis", Errors(Command() with { ServiceDateBasis = (ServiceDateBasis)9 }));
        Assert.Contains("PricingUnit", Errors(Command() with { PricingUnit = (PricingUnit)99 }));
    }
}
