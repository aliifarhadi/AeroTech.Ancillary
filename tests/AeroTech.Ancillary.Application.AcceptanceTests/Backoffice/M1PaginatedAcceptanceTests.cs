using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class M1PaginatedAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1PaginatedAcceptanceTests(TestDatabase database) => _database = database;

    [Fact]
    public async Task M1_A01_suppliers_are_listed_as_a_grid_filtered_by_airline_kind_status_and_search()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var dotAir = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Lounge"));
        await scope.RegisterSupplier.RegisterAsync(M1Commands.ExternalSupplier(airlineId));

        var all = await scope.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId });

        Assert.Equal(3, all.TotalCount);
        Assert.Equal(new[] { "Dot Air", "IKA CIP Lounge", "Partner Lounge" }, all.Results.Select(row => row.Name));
        Assert.Equal(
            new[] { "Airline", "Name", "Fulfillment", "Provider Key", "Status", "Created" },
            all.Metadata.Fields.Select(field => field.Title));

        var first = all.Results.First();
        Assert.Equal(dotAir.Id.ToString(), first.Id);
        Assert.Equal("Local", first.FulfillmentKind.Name);
        Assert.Equal("Active", first.Status.Name);

        var external = await scope.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, FulfillmentKind = SupplierFulfillmentKind.External });

        Assert.Equal("LoungePartnerA", Assert.Single(external.Results).FulfillmentProviderKey);

        var searched = await scope.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, Search = "CIP" });

        Assert.Equal("IKA CIP Lounge", Assert.Single(searched.Results).Name);

        var retired = await scope.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, Status = SupplierStatus.Retired });

        Assert.Equal(0, retired.TotalCount);

        var secondPage = await scope.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, PageNumber = 2, PageSize = 2 });

        Assert.Equal(3, secondPage.TotalCount);
        Assert.Equal(2, secondPage.PageNumber);
        Assert.Equal("Partner Lounge", Assert.Single(secondPage.Results).Name);
    }

    [Fact]
    public async Task M1_C09_service_definitions_are_listed_by_reference_then_newest_version_with_their_filters()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var dotAir = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var cip = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Lounge"));
        var firstVersion = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, dotAir.Id, "LNG_DOTAIR"));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(firstVersion.Id));
        var secondVersion = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, dotAir.Id, "LNG_DOTAIR"));
        var partner = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, cip.Id, "LNG_CIP"));

        var all = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId });

        Assert.Equal(3, all.TotalCount);
        Assert.Equal(
            new[] { partner.Id.ToString(), secondVersion.Id.ToString(), firstVersion.Id.ToString() },
            all.Results.Select(row => row.Id));
        Assert.Equal(new[] { 1, 2, 1 }, all.Results.Select(row => row.Version));
        Assert.Contains("Supplier", all.Metadata.Fields.Select(field => field.Title));

        var row = all.Results.Last();
        Assert.Equal("LNG_DOTAIR", row.ServiceDefinitionRef);
        Assert.Equal("0BX", row.ServiceSubCode);
        Assert.Equal("F", row.ServiceTypeCode);
        Assert.Equal("LG", row.GroupCode);
        Assert.Equal(dotAir.Id.ToString(), row.SupplierId);
        Assert.Equal("Dot Air", row.SupplierName);
        Assert.Equal("Industry", row.SubCodeSource.Name);
        Assert.Equal("EmdStandalone", row.DocumentType.Name);
        Assert.Equal("Active", row.Status.Name);

        var bySupplier = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, SupplierId = cip.Id });

        Assert.Equal(partner.Id.ToString(), Assert.Single(bySupplier.Results).Id);

        var active = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, Status = ServiceDefinitionStatus.Active });

        Assert.Equal(firstVersion.Id.ToString(), Assert.Single(active.Results).Id);

        var byReference = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, ServiceDefinitionRef = "LNG_DOTAIR" });

        Assert.Equal(2, byReference.TotalCount);

        var byClassification = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery
            {
                OwnerAirlineId = airlineId,
                ServiceSubCode = "0BX",
                ServiceTypeCode = "F",
                GroupCode = "LG"
            });

        Assert.Equal(3, byClassification.TotalCount);

        var searched = await scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, Search = "CIP" });

        Assert.Equal(partner.Id.ToString(), Assert.Single(searched.Results).Id);
    }

    [Fact]
    public async Task M1_D02_provisions_of_a_definition_are_listed_by_sequence_and_their_prices_by_version_with_currency_code()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var dotAir = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, dotAir.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));
        var otherDefinition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, dotAir.Id, "LNG_OTHER"));
        var first = await scope.DefineProvision.DefineAsync(
            M1Commands.LoungeProvision(definition.Id, sequence: 100, disposition: CommercialDisposition.Paid));
        var firstPrice = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(first.Id, 15000000m));
        await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(first.Id, firstPrice.Id));
        var later = await scope.DefineProvision.DefineAsync(
            M1Commands.LoungeProvision(definition.Id, sequence: 200, disposition: CommercialDisposition.Paid));
        var laterPrice = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(later.Id, 18000000m));
        await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(otherDefinition.Id));

        var all = await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definition.Id });

        Assert.Equal(2, all.TotalCount);
        Assert.Equal(new[] { first.Id.ToString(), later.Id.ToString() }, all.Results.Select(row => row.Id));
        Assert.Equal(
            new[]
            {
                "Sequence", "Coverage", "Disposition", "Unit", "Min", "Max", "Permitted Periods", "Blackouts", "Day/Time", "Sales From",
                "Sales Until", "Status", "Created"
            },
            all.Metadata.Fields.Select(field => field.Title));

        var row = all.Results.First();
        Assert.Equal(definition.Id.ToString(), row.ServiceDefinitionId);
        Assert.Equal(100, row.Sequence);
        Assert.Equal("Sector", row.CoverageScope.Name);
        Assert.Equal("Paid", row.Disposition.Name);
        Assert.Equal("Each", row.QuantityUnit.Name);
        Assert.Equal((0, 0, 0), (row.PermittedPeriodCount, row.BlackoutPeriodCount, row.DayTimeWindowCount));
        Assert.Equal("Active", row.Status.Name);

        var prices = await scope.GetPricingsPaginated.ExecuteAsync(new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = first.Id });

        Assert.Equal(
            new[] { "Version", "Pricing Unit", "Currencies", "Rates", "Status", "Created", "Activated" },
            prices.Metadata.Fields.Select(field => field.Title));
        Assert.Equal(
            (firstPrice.Id.ToString(), first.Id.ToString(), 1, "PerPassenger", "IRR", 1, "Active"),
            prices.Results.Select(price => (price.Id, price.AncillaryProvisionId, price.Version, price.PricingUnit!.Name, price.Currencies, price.RateCount, price.Status.Name)).Single());
        Assert.Equal(15000000m, Assert.Single((await scope.GetPricingById.ExecuteAsync(firstPrice.Id)).PriceLines).Amount);

        var draftPrices = await scope.GetPricingsPaginated.ExecuteAsync(new BackofficeGetAncillaryPricingsPaginatedQuery { Status = PricingStatus.Draft, AncillaryProvisionId = later.Id });

        Assert.Equal(laterPrice.Id.ToString(), Assert.Single(draftPrices.Results).Id);

        var drafts = await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definition.Id, Status = ProvisionStatus.Draft });

        Assert.Equal(later.Id.ToString(), Assert.Single(drafts.Results).Id);

        var bySequence = await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definition.Id, Sequence = 200 });

        Assert.Equal(later.Id.ToString(), Assert.Single(bySequence.Results).Id);

        var bySupplier = await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definition.Id, SupplierId = dotAir.Id + 1 });

        Assert.Equal(0, bySupplier.TotalCount);

        var bySalesDate = await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definition.Id, SalesDate = _clock.Now });

        Assert.Equal(2, bySalesDate.TotalCount);

        var withoutDefinition = await scope.GetProvisionsPaginated.ExecuteAsync(new BackofficeGetAncillaryProvisionsPaginatedQuery());

        Assert.Equal(0, withoutDefinition.TotalCount);
    }
}
