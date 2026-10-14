using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class V121PriceStressAcceptanceTests
{
    private const long AirlineOffice = 9200000000000001;
    private const long AgencyOffice = 1551571720488353792;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121PriceStressAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<long> DefinitionAsync(string reference, PricingUnit pricingUnit = PricingUnit.PerPassenger, ServiceDateBasis basis = ServiceDateBasis.FlightDeparture)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "SVC", "F", "TS", reference, pricingUnit: pricingUnit, serviceDateBasis: basis))).Id;
    }

    private async Task<(int Sequence, string Rules, string Filed)[]> FiledAsync(long serviceDefinitionId)
    {
        var filed = new List<(int, string, string)>();

        await using var reader = new AncillaryScope(_database, _clock);

        foreach (var row in (await _proof.ListedProvisionsAsync(serviceDefinitionId)).Where(row => row.Status.Name == "Active").OrderBy(row => row.Sequence))
        {
            var provisionId = long.Parse(row.Id);
            var active = await reader.Pricings.FindActiveAsync(provisionId);

            filed.Add((
                row.Sequence,
                RuleText.Of(await reader.GetProvisionById.ExecuteAsync(provisionId)),
                active is null ? row.Disposition.Name : RuleText.Of(await reader.GetPricingById.ExecuteAsync(active.Id))));
        }

        return filed.ToArray();
    }
}
