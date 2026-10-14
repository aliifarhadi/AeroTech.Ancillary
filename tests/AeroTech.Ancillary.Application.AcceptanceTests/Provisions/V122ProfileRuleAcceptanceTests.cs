using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V122ProfileRuleAcceptanceTests
{
    private static readonly string[] SharedSections = ["PassengerEligibility", "SalesRestrictions", "Geography", "TravelDate", "DayTimeApplication", "AdvancePurchase"];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V122ProfileRuleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
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

    private async Task<(long DefinitionId, long ProvisionId, TestDefineProvisionCommand Command)> DraftRuleAsync(string code)
    {
        var variant = V122Catalog.Case(code);
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(
            V122Catalog.NeedsExternalSupplier(variant) ? M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider) : M1Commands.LocalSupplier(airlineId));
        var definition = await _proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId));
        var command = V122Catalog.Rule(variant, definition.Id) with
        {
            PetRule = null,
            AssistedTravelRule = null,
            AirportServiceRule = null
        };
        var provision = await RequestAsync(scope => scope.DefineProvision.DefineAsync(command));

        return (definition.Id, provision.Id, command);
    }

    private Task<BackofficeProvisionDto> ReadAsync(long provisionId) => RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));
}
