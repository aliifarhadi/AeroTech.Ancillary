using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class M1ProvisionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1ProvisionAcceptanceTests(TestDatabase database) => _database = database;

    private async Task<(long DefinitionId, AncillaryScope Scope)> ActiveDefinitionAsync()
    {
        var airlineId = _database.NextAirlineId();
        var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));

        return (definition.Id, scope);
    }

    private static TestDefineProvisionCommand PaidLounge(long definitionId)
        => M1Commands.LoungeProvision(definitionId, disposition: CommercialDisposition.Paid);

    [Fact]
    public async Task M1_D01_a_provision_is_defined_as_draft_with_its_price_lines()
    {
        var (definitionId, scope) = await ActiveDefinitionAsync();
        await using var _ = scope;

        var draft = await scope.DefineProvision.DefineAsync(PaidLounge(definitionId));
        var pricing = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(draft.Id));

        Assert.Equal(ProvisionStatus.Draft, draft.Status);
        Assert.Equal(ServiceCoverageScope.Sector, draft.CoverageScope);
        Assert.Equal(CommercialDisposition.Paid, draft.Disposition);
        Assert.Equal((PricingStatus.Draft, 1, draft.Id), (pricing.Status, pricing.Version, pricing.AncillaryProvisionId));

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);
        var price = await reader.GetPricingById.ExecuteAsync(pricing.Id);

        Assert.Equal(definitionId, detail.ServiceDefinitionId);
        Assert.Equal(100, detail.Sequence);
        Assert.Equal("Sector", detail.CoverageScope.Name);
        Assert.Equal("Each", detail.QuantityUnit.Name);
        Assert.Equal("Ancillary", detail.FulfillmentProviderKey);
        Assert.Equal(M1Commands.Currency, price.CurrencyId);
        Assert.Equal("Item", price.FeeApplicationUnit!.Name);

        var line = Assert.Single(price.PriceLines);

        Assert.Equal(2500000m, line.Amount);
        Assert.Equal("Lounge access", line.Name);
    }

    [Fact]
    public async Task M1_D01_a_provision_for_a_missing_definition_is_refused()
    {
        await using var scope = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16304, 422, () => scope.DefineProvision.DefineAsync(
            M1Commands.LoungeProvision(999_999_999)));
    }

    [Fact]
    public async Task M1_D02_M06_activation_supersedes_the_previous_active_sequence_atomically()
    {
        var (definitionId, scope) = await ActiveDefinitionAsync();
        await using var _ = scope;

        var first = await scope.DefineProvision.DefineAsync(PaidLounge(definitionId));
        var firstPrice = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(first.Id, 2500000m));

        await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(first.Id, firstPrice.Id));

        var revision = await scope.DefineProvision.DefineAsync(PaidLounge(definitionId));
        var revisionPrice = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(revision.Id, 2800000m));

        Assert.NotEqual(first.Id, revision.Id);

        var activated = await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(revision.Id, revisionPrice.Id));

        Assert.Equal(ProvisionStatus.Active, activated.Status);

        await using var reader = new AncillaryScope(_database, _clock);
        var superseded = await reader.GetProvisionById.ExecuteAsync(first.Id);
        var current = await reader.GetProvisionById.ExecuteAsync(revision.Id);

        Assert.Equal("Retired", superseded.Status.Name);
        Assert.Equal("Active", current.Status.Name);
        Assert.Equal(2500000m, Assert.Single((await reader.GetPricingById.ExecuteAsync(firstPrice.Id)).PriceLines).Amount);
        Assert.Equal(2800000m, Assert.Single((await reader.GetPricingById.ExecuteAsync(revisionPrice.Id)).PriceLines).Amount);
    }

    [Fact]
    public async Task M1_G09A_an_unimplemented_fee_application_unit_is_refused_at_activation()
    {
        var (definitionId, scope) = await ActiveDefinitionAsync();
        await using var _ = scope;

        var draft = await scope.DefineProvision.DefineAsync(PaidLounge(definitionId));
        var pricing = await scope.DefinePricing.DefineAsync(
            M1Commands.LoungePricing(draft.Id, feeApplicationUnit: FeeApplicationUnit.PerFiveKilogramsOver));

        await BusinessAssert.ThrowsAsync(16305, 422, () => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(pricing.Id)));
        await BusinessAssert.ThrowsAsync(16305, 422, () => scope.PublishProvision.PublishAsync(
            new TestPublishProvisionCommand(draft.Id, pricing.Id)));
    }

    [Fact]
    public async Task M1_D02_the_unique_active_sequence_is_backed_by_a_filtered_index()
    {
        var (definitionId, scope) = await ActiveDefinitionAsync();
        await using var _ = scope;

        var indexes = await scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT i.name AS Value FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id " +
                "WHERE t.name = 'AncillaryProvisions' AND i.has_filter = 1 AND i.is_unique = 1")
            .ToListAsync();

        Assert.Contains("IX_AncillaryProvisions_ServiceDefinitionId_Sequence_Active", indexes);
    }
}
