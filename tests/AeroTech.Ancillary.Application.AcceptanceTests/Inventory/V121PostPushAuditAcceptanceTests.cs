using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class V121PostPushAuditAcceptanceTests
{
    private const long Flight = 81234;
    private const int ProbeCurrency = 9901;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V121PostPushAuditAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task SetProbeDecimalPlacesAsync(int? decimalPlaces)
    {
        await using var reference = _database.NewReferenceContext();
        var stored = await reference.Currencies.FirstOrDefaultAsync(currency => currency.Id == ProbeCurrency);

        if (stored is not null)
            reference.Currencies.Remove(stored);

        if (decimalPlaces is { } places)
            reference.Currencies.Add(new CurrencyReadModel { Id = ProbeCurrency, Code = "XTS", DecimalPlaces = places, RoundingFactor = 0 });

        await reference.SaveChangesAsync();
    }

    private async Task<long> NextVersionAsync(long definitionId)
    {
        await using var author = new AncillaryScope(_database, _clock);
        var revisionId = (await author.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(definitionId))).Id;

        await author.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(definitionId));
        await author.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(revisionId));

        return revisionId;
    }
}
