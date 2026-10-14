using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V121ProvisionRuleAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121ProvisionRuleAcceptanceTests(TestDatabase database)
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

    private async Task<long> ActiveDefinitionAsync(string reference = "PRIORITY_BOARDING")
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "PRB", "F", "TS", "Priority boarding"))).Id;
    }

    private static TestDefineProvisionCommand Free(long definitionId, int sequence = 10) => Provision(definitionId, sequence, CommercialDisposition.Free);

    private static (DateOnly Start, DateOnly End)[] Periods(IReadOnlyList<BackofficeProvisionDatePeriodDto> rows)
        => rows.Select(row => (row.StartDate, row.EndDate)).ToArray();

    private Task<List<string>> RowsAsync(FormattableString sql) => RequestAsync(scope => scope.Command.Database.SqlQuery<string>(sql).ToListAsync());

    private Task<BackofficeProvisionDto> ReadAsync(long provisionId) => RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));

    private static TestDefineProvisionCommand FullyRestricted(long definitionId)
        => Provision(definitionId, 10, CommercialDisposition.Free, applicationType: ProvisionApplicationType.Seat) with
        {
            PassengerEligibility = new([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [new(2, 12), new(12, null)]),
            SalesRestrictions = new(
                new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                new DateTimeOffset(2027, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                [501, 502],
                [9001],
                [CustomerType.TravelAgency, CustomerType.Organization]),
            Geography = new(
                [Thr],
                [Ist],
                [Mhd],
                [Pair(Thr, Ist), Pair(Mhd, Ist, RoutePairDirection.BothDirections)],
                [Location(ServiceLocationType.Airport, Thr), Location(ServiceLocationType.City, 7)],
                [98, 90]),
            FlightApplication = new([1, 2], [3], [" w5112", "W5116"], [81234, 81240], [1, 2]),
            FareApplication = new([7001], [AirFareType.Public, AirFareType.Private], [5, 6], ["y26lt"], [2], [41, 42]),
            TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]),
            DayTimeApplication = DayTime(Window(Monday, 8, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny)),
            AdvancePurchase = new(3, TimeUnit.Days),
            SeatApplication = Seat(["12a", "12F"], ["w", "LS"])
        };

    private static object?[] Shape(BackofficeProvisionDto detail)
        =>
        [
            string.Join(",", detail.PassengerEligibility?.AllowedPassengerTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.PassengerEligibility?.AllowedAgeBands.Select(row => $"{row.AgeFromInclusive}-{row.AgeToExclusive}") ?? []),
            detail.SalesRestrictions?.SalesEffectiveFrom,
            detail.SalesRestrictions?.SalesDiscontinueAt,
            string.Join(",", detail.SalesRestrictions?.AllowedPointsOfSale.Select(row => row.Value) ?? []),
            string.Join(",", detail.SalesRestrictions?.AllowedCustomers.Select(row => row.Value) ?? []),
            string.Join(",", detail.SalesRestrictions?.AllowedCustomerTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.Geography?.AllowedOriginAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedDestinationAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedViaAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedRoutePairs.Select(row => $"{row.OriginAirportId}>{row.DestinationAirportId}:{row.Direction.Name}") ?? []),
            string.Join(",", detail.Geography?.ServiceLocations.Select(row => $"{row.LocationType.Name}:{row.LocationId}") ?? []),
            string.Join(",", detail.Geography?.CoverageCountries.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedMarketingAirlines.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedOperatingAirlines.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedFlightNumbers.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedFlights.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedAircraft.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedAirFares.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedAirFareTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.FareApplication?.AllowedFareFamilies.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedFareBases.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedCabinClasses.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedRbds.Select(row => row.Value) ?? []),
            string.Join(",", detail.TravelDate?.PermittedPeriods.Select(row => $"{row.StartDate:yyyy-MM-dd}..{row.EndDate:yyyy-MM-dd}") ?? []),
            string.Join(",", detail.TravelDate?.BlackoutPeriods.Select(row => $"{row.StartDate:yyyy-MM-dd}..{row.EndDate:yyyy-MM-dd}") ?? []),
            string.Join(",", detail.DayTimeApplication?.Windows.Select(row => $"{row.DaysOfWeekMask}:{row.StartLocalTime?.Hour}-{row.EndLocalTime?.Hour}:{row.Effect.Name}") ?? []),
            detail.AdvancePurchase is null ? null : $"{detail.AdvancePurchase.MinimumPeriod}:{detail.AdvancePurchase.Unit.Name}:{detail.AdvancePurchase.SameTimeAsTicketed}",
            detail.BaggageApplication is null ? null : $"{detail.BaggageApplication.Weight}:{detail.BaggageApplication.WeightUnit.Name}:{detail.BaggageApplication.PurchaseApplication.Name}",
            string.Join(",", detail.SeatApplication?.SeatNumbers.Select(row => row.Value) ?? []),
            string.Join(",", detail.SeatApplication?.SeatCharacteristics.Select(row => row.Value) ?? [])
        ];

    private static long[] RowIds(BackofficeProvisionDto detail)
        => new[]
            {
                detail.PassengerEligibility?.AllowedPassengerTypes.Select(row => row.Id), detail.PassengerEligibility?.AllowedAgeBands.Select(row => row.Id),
                detail.SalesRestrictions?.AllowedPointsOfSale.Select(row => row.Id), detail.SalesRestrictions?.AllowedCustomers.Select(row => row.Id),
                detail.SalesRestrictions?.AllowedCustomerTypes.Select(row => row.Id), detail.Geography?.AllowedOriginAirports.Select(row => row.Id),
                detail.Geography?.AllowedDestinationAirports.Select(row => row.Id), detail.Geography?.AllowedViaAirports.Select(row => row.Id),
                detail.Geography?.AllowedRoutePairs.Select(row => row.Id), detail.Geography?.ServiceLocations.Select(row => row.Id),
                detail.Geography?.CoverageCountries.Select(row => row.Id), detail.FlightApplication?.AllowedMarketingAirlines.Select(row => row.Id),
                detail.FlightApplication?.AllowedOperatingAirlines.Select(row => row.Id), detail.FlightApplication?.AllowedFlightNumbers.Select(row => row.Id),
                detail.FlightApplication?.AllowedFlights.Select(row => row.Id), detail.FlightApplication?.AllowedAircraft.Select(row => row.Id),
                detail.FareApplication?.AllowedAirFares.Select(row => row.Id), detail.FareApplication?.AllowedAirFareTypes.Select(row => row.Id),
                detail.FareApplication?.AllowedFareFamilies.Select(row => row.Id), detail.FareApplication?.AllowedFareBases.Select(row => row.Id),
                detail.FareApplication?.AllowedCabinClasses.Select(row => row.Id), detail.FareApplication?.AllowedRbds.Select(row => row.Id),
                detail.TravelDate?.PermittedPeriods.Select(row => row.Id), detail.TravelDate?.BlackoutPeriods.Select(row => row.Id),
                detail.DayTimeApplication?.Windows.Select(row => row.Id), detail.SeatApplication?.SeatNumbers.Select(row => row.Id),
                detail.SeatApplication?.SeatCharacteristics.Select(row => row.Id)
            }
            .SelectMany(ids => ids ?? [])
            .ToArray();

    private static long?[] RuleIds(BackofficeProvisionDto detail)
        =>
        [
            detail.PassengerEligibility?.Id, detail.SalesRestrictions?.Id, detail.Geography?.Id, detail.FlightApplication?.Id, detail.FareApplication?.Id,
            detail.TravelDate?.Id, detail.DayTimeApplication?.Id, detail.AdvancePurchase?.Id, detail.BaggageApplication?.Id, detail.SeatApplication?.Id
        ];

    [Fact]
    public async Task V121_E17_two_drafts_racing_for_the_same_active_sequence_end_with_one_active_and_a_conflict()
    {
        var definitionId = await ActiveDefinitionAsync();
        var first = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;
        var second = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;

        await using var winner = new AncillaryScope(_database, _clock);
        await using var loser = new AncillaryScope(_database, _clock);

        var winning = (await winner.Provisions.GetAsync(first.Id))!;
        var losing = (await loser.Provisions.GetAsync(second.Id))!;
        var definition = (await winner.Definitions.GetAsync(definitionId))!;

        winning.Activate(definition, null, _clock.Now);
        losing.Activate(definition, null, _clock.Now);

        await winner.UnitOfWork.SaveChangesAsync();
        await BusinessAssert.ThrowsAsync(16306, 409, () => loser.UnitOfWork.SaveChangesAsync());

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal(
            new[] { first.Id },
            await reader.Command.AncillaryProvisions.AsNoTracking()
                .Where(row => row.ServiceDefinitionId == definitionId && row.Status == ProvisionStatus.Active)
                .Select(row => row.Id)
                .ToListAsync());
        Assert.Equal(ProvisionStatus.Draft, (await reader.Command.AncillaryProvisions.AsNoTracking().SingleAsync(row => row.Id == second.Id)).Status);
    }

    [Fact]
    public async Task V121_M07_two_concurrent_edits_of_one_draft_conflict_instead_of_overwriting()
    {
        var definitionId = await ActiveDefinitionAsync();
        var draftCommand = Free(definitionId) with { TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 10))]), PassengerEligibility = Passengers(PassengerTypeCode.ADT) };
        var draft = (await _proof.RuleAsync(draftCommand, publish: false)).Provision;

        await using (var firstEditor = new AncillaryScope(_database, _clock))
        await using (var secondEditor = new AncillaryScope(_database, _clock))
        {
            await firstEditor.Provisions.GetAsync(draft.Id);
            await secondEditor.Provisions.GetAsync(draft.Id);
            await firstEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 20 }));
            await BusinessAssert.ThrowsAsync(16005, 409, () => secondEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 30 })));
        }

        var stored = await ReadAsync(draft.Id);

        Assert.Equal(20, stored.Sequence);
        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 4, 10)) }, Periods(stored.TravelDate!.PermittedPeriods));
        Assert.Equal("ADT", stored.PassengerEligibility!.AllowedPassengerTypes.Single().Value.Name);
    }
}
