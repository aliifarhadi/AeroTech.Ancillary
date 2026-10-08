using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Migration;

public class V12MigrationAcceptanceTests : IAsyncLifetime
{
    private const string V11Command = "20261007202914_V11Phase1CommercialAuthoring";
    private const string V11Query = "20261007202918_V11Phase1CommercialAuthoringQuery";
    private const string V12Command = "20261008103942_V12Phase1NormalizedProvisionAndPricing";
    private const string V12Query = "20261008103947_V12Phase1NormalizedProvisionAndPricingQuery";

    private const long AirlineOffice = 9200000000000001;
    private const long AgencyOffice = 1551571720488353792;
    private const long BagDefinition = 9101;
    private const long SimDefinition = 9102;
    private const long FullRule = 9201;
    private const long DraftRule = 9202;
    private const long FreeRule = 9203;
    private const long RetiredRule = 9204;
    private const long SuspendedRule = 9205;
    private const long SeatRule = 9206;
    private const long SimRule = 9207;

    private const string V11Seed =
        """
        DECLARE @created datetimeoffset = '2026-09-01T08:00:00+00:00';
        DECLARE @activated datetimeoffset = '2026-09-02T08:00:00+00:00';
        DECLARE @suspended datetimeoffset = '2026-09-03T08:00:00+00:00';
        DECLARE @retired datetimeoffset = '2026-09-04T08:00:00+00:00';

        INSERT INTO [Ancillary].[Suppliers] ([Id], [OwnerAirlineId], [Name], [FulfillmentKind], [FulfillmentProviderKey], [Status], [CreatedAt], [RetiredAt], [LastUpdateTime], [LastUpdatedBy])
        VALUES (9001, 7, N'Dot Air', 1, NULL, 1, @created, NULL, @created, 42);

        INSERT INTO [ReadModel].[Suppliers] ([Id], [OwnerAirlineId], [Name], [FulfillmentKind], [FulfillmentProviderKey], [Status], [CreatedAt], [RetiredAt], [LastUpdateTime])
        SELECT [Id], [OwnerAirlineId], [Name], [FulfillmentKind], [FulfillmentProviderKey], [Status], [CreatedAt], [RetiredAt], [LastUpdateTime]
        FROM [Ancillary].[Suppliers];

        INSERT INTO [Ancillary].[AncillaryServiceDefinitions] ([Id], [OwnerAirlineId], [SupplierId], [ServiceDefinitionRef], [Version], [ServiceTypeCode], [ServiceSubCode], [SubCodeSource], [GroupCode], [SubGroupCode], [Description1Code], [Description2Code], [CommercialName], [Description], [DocumentType], [DocumentRfic], [DocumentRfisc], [BookingMethod], [BookingSsrCode], [BookingSsimCode], [SalesEffectiveFrom], [SalesDiscontinueOn], [Status], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (9101, 7, 9001, N'XBAG_PIECE_23KG', 1, N'C', N'0CC', 1, N'BG', NULL, N'B1', NULL, N'First excess bag', N'One additional checked piece', 2, N'C', N'0CC', 1, N'XBAG', NULL, NULL, NULL, 2, @created, @activated, NULL, NULL, @activated, 42),
            (9102, 7, 9001, N'SIM_CARD', 1, N'M', N'SIM', 2, N'ST', NULL, NULL, NULL, N'Travel SIM card', NULL, 1, NULL, NULL, 4, NULL, NULL, '2026-10-01', '2027-09-30', 1, @created, NULL, NULL, NULL, @created, 42);

        INSERT INTO [ReadModel].[AncillaryServiceDefinitions] ([Id], [OwnerAirlineId], [SupplierId], [SupplierName], [ServiceDefinitionRef], [Version], [ServiceTypeCode], [ServiceSubCode], [SubCodeSource], [GroupCode], [SubGroupCode], [Description1Code], [Description2Code], [CommercialName], [Description], [DocumentType], [DocumentRfic], [DocumentRfisc], [BookingMethod], [BookingSsrCode], [BookingSsimCode], [SalesEffectiveFrom], [SalesDiscontinueOn], [Status], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime])
        SELECT [Id], [OwnerAirlineId], [SupplierId], N'Dot Air', [ServiceDefinitionRef], [Version], [ServiceTypeCode], [ServiceSubCode], [SubCodeSource], [GroupCode], [SubGroupCode], [Description1Code], [Description2Code], [CommercialName], [Description], [DocumentType], [DocumentRfic], [DocumentRfisc], [BookingMethod], [BookingSsrCode], [BookingSsimCode], [SalesEffectiveFrom], [SalesDiscontinueOn], [Status], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime]
        FROM [Ancillary].[AncillaryServiceDefinitions];

        INSERT INTO [Ancillary].[AncillaryProvisions] ([Id], [ServiceDefinitionId], [Sequence], [Status], [CoverageScope], [QuantityUnit], [MinQuantity], [MaxQuantity], [ApplicationType], [Disposition], [DocumentRequired], [BookingRequired], [FeeCurrencyId], [FeeApplicationUnit], [ReissueRefund], [FormOfRefund], [Commissionable], [InterlineSettlement], [MustCheckAvailability], [CreatedAt], [ActivatedAt], [SuspendedAt], [RetiredAt], [LastUpdateTime], [LastUpdatedBy], [FulfillmentProviderKey], [BaggageWeight], [BaggageWeightUnit], [BaggagePurchaseApplication])
        VALUES
            (9201, 9101, 10, 2, 3, 2, 1, 3, 2, 1, 1, 0, 47, 3, 2, NULL, 0, 0, 0, @created, @activated, NULL, NULL, @activated, 42, N'Ancillary', 23.50, 1, 1),
            (9202, 9101, 20, 1, 3, 2, 1, 1, 2, 1, 1, 0, 70, 3, 2, NULL, 0, 0, 0, @created, NULL, NULL, NULL, @created, 42, N'Ancillary', 23.00, 1, 1),
            (9203, 9101, 30, 2, 3, 2, 1, 1, 2, 2, 0, 0, NULL, NULL, 2, NULL, 0, 0, 0, @created, @activated, NULL, NULL, @activated, 42, N'Ancillary', 23.00, 1, 1),
            (9204, 9101, 40, 4, 3, 2, 1, 1, 2, 1, 1, 0, 155, 3, 2, NULL, 0, 0, 0, @created, @activated, NULL, @retired, @retired, 42, N'Ancillary', 23.00, 1, 1),
            (9205, 9101, 50, 3, 3, 2, 1, 1, 2, 1, 1, 0, 47, 3, 2, NULL, 0, 0, 0, @created, @activated, @suspended, NULL, @suspended, 42, N'Ancillary', 23.00, 1, 1),
            (9206, 9101, 60, 2, 1, 1, 1, 1, 3, 1, 1, 0, 47, 3, 2, NULL, 0, 0, 0, @created, @activated, NULL, NULL, @activated, 42, N'Ancillary', NULL, NULL, NULL),
            (9207, 9102, 10, 1, 1, 1, 1, 1, 1, 1, 1, 0, 47, 3, 2, NULL, 0, 0, 0, @created, NULL, NULL, NULL, @created, 42, N'Ancillary', NULL, NULL, NULL);

        UPDATE [Ancillary].[AncillaryProvisions]
        SET [SalesEffectiveFrom] = '2026-11-01T00:00:00+03:30', [SalesDiscontinueAt] = '2026-12-01T00:00:00+03:30',
            [AdvancePurchasePeriod] = 3, [AdvancePurchaseUnit] = 3,
            [PassengerTypeCodes] = N'[1,27]', [PointOfSaleIds] = N'[9200000000000001,1551571720488353792]', [CustomerIds] = N'[9001]', [CustomerTypes] = N'[2,3]',
            [OriginAirportIds] = N'[1]', [DestinationAirportIds] = N'[6]', [ViaAirportIds] = N'[3]',
            [MarketingAirlineIds] = N'[1,2]', [OperatingAirlineIds] = N'[3]', [FlightNumbers] = N'["W5112","W5116"]', [FlightIds] = N'[81234,81240]', [AircraftIds] = N'[1,2]',
            [AirFareIds] = N'[7001]', [AirFareTypes] = N'[1,2]', [FareFamilyIds] = N'[5,6]', [FareBasisCodes] = N'["Y26LT"]', [CabinClassIds] = N'[2]', [RbdIds] = N'[41,42]',
            [TravelFrom] = '2026-12-20', [TravelTo] = '2026-12-31', [DaysOfWeek] = N'[1,5]', [TimeFrom] = '08:00', [TimeTo] = '12:00',
            [BaggageFreePieces] = 0, [BaggageFirstExcessPiece] = 1, [BaggageLastExcessPiece] = 2, [BaggageTravelApplication] = 1
        WHERE [Id] = 9201;

        UPDATE [Ancillary].[AncillaryProvisions] SET [TravelFrom] = '2026-12-01', [TimeFrom] = '06:00', [TimeTo] = '10:00' WHERE [Id] = 9202;
        UPDATE [Ancillary].[AncillaryProvisions] SET [PassengerTypeCodes] = N'[46]', [DaysOfWeek] = N'[3,4,5]' WHERE [Id] = 9203;
        UPDATE [Ancillary].[AncillaryProvisions] SET [DaysOfWeek] = N'[5]', [TimeFrom] = '22:00', [TimeTo] = '02:00' WHERE [Id] = 9204;
        UPDATE [Ancillary].[AncillaryProvisions] SET [AircraftIds] = N'[1]', [SeatNumbers] = N'["1A","1C"]', [SeatCharacteristicCodes] = N'["E"]' WHERE [Id] = 9206;

        INSERT INTO [ReadModel].[AncillaryProvisions] ([Id], [ServiceDefinitionId], [Sequence], [Status], [SalesEffectiveFrom], [SalesDiscontinueAt], [CoverageScope], [QuantityUnit], [MinQuantity], [MaxQuantity], [ApplicationType], [Disposition], [DocumentRequired], [BookingRequired], [FeeCurrencyId], [FeeApplicationUnit], [ReissueRefund], [FormOfRefund], [Commissionable], [InterlineSettlement], [MustCheckAvailability], [CreatedAt], [LastUpdateTime], [FulfillmentProviderKey], [ActivatedAt], [AdvancePurchasePeriod], [AdvancePurchaseUnit], [AirFareIds], [AirFareTypes], [AircraftIds], [BaggageFirstExcessPiece], [BaggageFreePieces], [BaggageLastExcessPiece], [BaggagePurchaseApplication], [BaggageRuleDeference], [BaggageTravelApplication], [BaggageWeight], [BaggageWeightUnit], [CabinClassIds], [CustomerIds], [CustomerTypes], [DaysOfWeek], [DestinationAirportIds], [FareBasisCodes], [FareFamilyIds], [FlightIds], [FlightNumbers], [MarketingAirlineIds], [OperatingAirlineIds], [OriginAirportIds], [PassengerTypeCodes], [PointOfSaleIds], [RbdIds], [RetiredAt], [SeatCharacteristicCodes], [SeatNumbers], [SuspendedAt], [TimeFrom], [TimeTo], [TravelFrom], [TravelTo], [ViaAirportIds])
        SELECT [Id], [ServiceDefinitionId], [Sequence], [Status], [SalesEffectiveFrom], [SalesDiscontinueAt], [CoverageScope], [QuantityUnit], [MinQuantity], [MaxQuantity], [ApplicationType], [Disposition], [DocumentRequired], [BookingRequired], [FeeCurrencyId], [FeeApplicationUnit], [ReissueRefund], [FormOfRefund], [Commissionable], [InterlineSettlement], [MustCheckAvailability], [CreatedAt], [LastUpdateTime], [FulfillmentProviderKey], [ActivatedAt], [AdvancePurchasePeriod], [AdvancePurchaseUnit], [AirFareIds], [AirFareTypes], [AircraftIds], [BaggageFirstExcessPiece], [BaggageFreePieces], [BaggageLastExcessPiece], [BaggagePurchaseApplication], [BaggageRuleDeference], [BaggageTravelApplication], [BaggageWeight], [BaggageWeightUnit], [CabinClassIds], [CustomerIds], [CustomerTypes], [DaysOfWeek], [DestinationAirportIds], [FareBasisCodes], [FareFamilyIds], [FlightIds], [FlightNumbers], [MarketingAirlineIds], [OperatingAirlineIds], [OriginAirportIds], [PassengerTypeCodes], [PointOfSaleIds], [RbdIds], [RetiredAt], ISNULL([SeatCharacteristicCodes], N'[]'), ISNULL([SeatNumbers], N'[]'), [SuspendedAt], [TimeFrom], [TimeTo], [TravelFrom], [TravelTo], [ViaAirportIds]
        FROM [Ancillary].[AncillaryProvisions];

        INSERT INTO [Ancillary].[ProvisionPriceLines] ([Id], [AncillaryProvisionId], [Category], [Code], [Name], [UnitAmount], [CountryId], [StationAirportId], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (9301, 9201, 1, NULL, N'Extra bag', 30.00, NULL, NULL, @created, 42),
            (9302, 9201, 2, N'VAT', N'Value added tax', 2.70, 98, 1, @created, 42),
            (9303, 9201, 3, N'HDL', N'Handling', 1.05, NULL, NULL, @created, 42),
            (9304, 9202, 1, NULL, N'Extra bag', 15000000.00, NULL, NULL, @created, 42),
            (9305, 9204, 1, NULL, N'Extra bag', 22.00, NULL, NULL, @created, 42),
            (9306, 9205, 1, NULL, N'Extra bag', 9.99, NULL, NULL, @created, 42),
            (9307, 9206, 1, NULL, N'Seat', 12.00, NULL, NULL, @created, 42),
            (9308, 9207, 1, NULL, N'SIM card', 9.00, NULL, NULL, @created, 42),
            (9309, 9205, 3, NULL, N'Handling', 1.00, NULL, NULL, @created, 42);

        INSERT INTO [ReadModel].[AncillaryProvisionPriceLines] ([Id], [AncillaryProvisionId], [Category], [Code], [Name], [UnitAmount], [CountryId], [StationAirportId])
        SELECT [Id], [AncillaryProvisionId], [Category], [Code], [Name], [UnitAmount], [CountryId], [StationAirportId]
        FROM [Ancillary].[ProvisionPriceLines];

        INSERT INTO [Ancillary].[ProvisionRoutePairs] ([Id], [AncillaryProvisionId], [OriginAirportId], [DestinationAirportId], [Direction], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (9401, 9201, 1, 6, 1, @created, 42),
            (9402, 9201, 3, 6, 2, @created, 42);

        INSERT INTO [ReadModel].[AncillaryProvisionRoutePairs] ([Id], [AncillaryProvisionId], [OriginAirportId], [DestinationAirportId], [Direction])
        SELECT [Id], [AncillaryProvisionId], [OriginAirportId], [DestinationAirportId], [Direction]
        FROM [Ancillary].[ProvisionRoutePairs];
        """;

    private static readonly (string Command, string ReadModel)[] ConditionTables =
    [
        ("ProvisionPassengerTypes", "AncillaryProvisionPassengerTypes"),
        ("ProvisionPointsOfSale", "AncillaryProvisionPointsOfSale"),
        ("ProvisionCustomers", "AncillaryProvisionCustomers"),
        ("ProvisionCustomerTypes", "AncillaryProvisionCustomerTypes"),
        ("ProvisionOriginAirports", "AncillaryProvisionOriginAirports"),
        ("ProvisionDestinationAirports", "AncillaryProvisionDestinationAirports"),
        ("ProvisionViaAirports", "AncillaryProvisionViaAirports"),
        ("ProvisionRoutePairs", "AncillaryProvisionRoutePairs"),
        ("ProvisionMarketingAirlines", "AncillaryProvisionMarketingAirlines"),
        ("ProvisionOperatingAirlines", "AncillaryProvisionOperatingAirlines"),
        ("ProvisionFlightNumbers", "AncillaryProvisionFlightNumbers"),
        ("ProvisionFlights", "AncillaryProvisionFlights"),
        ("ProvisionAircraft", "AncillaryProvisionAircraft"),
        ("ProvisionAirFares", "AncillaryProvisionAirFares"),
        ("ProvisionAirFareTypes", "AncillaryProvisionAirFareTypes"),
        ("ProvisionFareFamilies", "AncillaryProvisionFareFamilies"),
        ("ProvisionFareBases", "AncillaryProvisionFareBases"),
        ("ProvisionCabinClasses", "AncillaryProvisionCabinClasses"),
        ("ProvisionRbds", "AncillaryProvisionRbds"),
        ("ProvisionTravelDates", "AncillaryProvisionTravelDates"),
        ("ProvisionSeasonalPeriods", "AncillaryProvisionSeasonalPeriods"),
        ("ProvisionBlackoutPeriods", "AncillaryProvisionBlackoutPeriods"),
        ("ProvisionDayTimeRestrictions", "AncillaryProvisionDayTimeRestrictions"),
        ("ProvisionSeatNumbers", "AncillaryProvisionSeatNumbers"),
        ("ProvisionSeatCharacteristics", "AncillaryProvisionSeatCharacteristics")
    ];

    private readonly TestDatabase _database = new();
    private readonly FixedClock _clock = new();
    private string[] _legacyBefore = [];

    public async Task InitializeAsync()
    {
        await using (var command = _database.NewContext(_clock))
            await command.GetService<IMigrator>().MigrateAsync(V11Command);

        await using (var query = _database.NewQueryContext())
            await query.GetService<IMigrator>().MigrateAsync(V11Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V11Seed);

        _legacyBefore = await LegacyRowsAsync();

        await _database.InitializeAsync();
    }

    public Task DisposeAsync() => _database.DisposeAsync();

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

    private Task<List<string>> RowsAsync(string sql)
        => RequestAsync(scope => scope.Command.Database.SqlQueryRaw<string>(sql).ToListAsync());

    private async Task<string[]> LegacyRowsAsync()
    {
        await using var context = _database.NewContext(_clock);

        return (await context.Database
                .SqlQueryRaw<string>(
                    "SELECT CONCAT('provision ', [Id], ' ', [Status], ' ', [Disposition], ' ', [FeeCurrencyId], ' ', [FeeApplicationUnit], ' ', [PassengerTypeCodes], ' ', [PointOfSaleIds], ' ', " +
                    "[FlightNumbers], ' ', [DaysOfWeek], ' ', [TravelFrom], ' ', [TravelTo], ' ', [TimeFrom], ' ', [TimeTo], ' ', [SeatNumbers]) AS Value FROM [Ancillary].[AncillaryProvisions] " +
                    "UNION ALL SELECT CONCAT('line ', [Id], ' ', [AncillaryProvisionId], ' ', [Category], ' ', [Code], ' ', [Name], ' ', [UnitAmount], ' ', [CountryId], ' ', [StationAirportId]) FROM [Ancillary].[ProvisionPriceLines] " +
                    "UNION ALL SELECT CONCAT('read-line ', [Id], ' ', [AncillaryProvisionId], ' ', [Category], ' ', [Code], ' ', [UnitAmount]) FROM [ReadModel].[AncillaryProvisionPriceLines] " +
                    "UNION ALL SELECT CONCAT('pair ', [Id], ' ', [AncillaryProvisionId], ' ', [OriginAirportId], ' ', [DestinationAirportId], ' ', [Direction]) FROM [Ancillary].[ProvisionRoutePairs] " +
                    "UNION ALL SELECT CONCAT('definition ', [Id], ' ', [ServiceDefinitionRef], ' ', [ServiceSubCode], ' ', [Status], ' ', [BookingSsrCode], ' ', [DocumentRfisc]) FROM [Ancillary].[AncillaryServiceDefinitions] " +
                    "UNION ALL SELECT CONCAT('supplier ', [Id], ' ', [Name], ' ', [Status]) FROM [Ancillary].[Suppliers]")
                .ToListAsync())
            .OrderBy(row => row, StringComparer.Ordinal)
            .ToArray();
    }

    [Fact]
    public async Task V12_C03_a_v11_database_is_migrated_with_every_selector_price_line_status_and_row_identity_preserved()
    {
        Assert.Equal(30, _legacyBefore.Length);
        Assert.Equal(_legacyBefore, await LegacyRowsAsync());
        Assert.Contains(V12Command, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.Contains(V12Query, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));

        foreach (var (command, readModel) in ConditionTables)
        {
            Assert.Empty(await RowsAsync(
                $"SELECT CONCAT([Id], ':', [AncillaryProvisionId]) AS Value FROM (" +
                $"(SELECT [Id], [AncillaryProvisionId] FROM [Ancillary].[{command}] EXCEPT SELECT [Id], [AncillaryProvisionId] FROM [ReadModel].[{readModel}]) " +
                $"UNION ALL (SELECT [Id], [AncillaryProvisionId] FROM [ReadModel].[{readModel}] EXCEPT SELECT [Id], [AncillaryProvisionId] FROM [Ancillary].[{command}])) AS difference"));
        }

        var full = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FullRule));
        var stored = (await RequestAsync(scope => scope.Provisions.GetAsync(FullRule)))!;

        Assert.Equal(("Active", BagDefinition, 10, "Journey", "Piece", 1, 3, "Paid"), (full.Status.Name, full.ServiceDefinitionId, full.Sequence, full.CoverageScope.Name, full.QuantityUnit.Name, full.MinQuantity, full.MaxQuantity, full.Disposition.Name));
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)), full.SalesEffectiveFrom);
        Assert.Equal((3, "Days"), (full.AdvancePurchase!.Period, full.AdvancePurchase.Unit.Name));
        Assert.Equal(("Baggage", 0, 1, 2, 23.5m, "Kg"), (full.ApplicationType.Name, full.Baggage!.FreePieces, full.Baggage.FirstExcessPiece, full.Baggage.LastExcessPiece, full.Baggage.Weight, full.Baggage.WeightUnit.Name));
        Assert.Equal(new[] { "ADT", "CHD" }, full.Passenger.PassengerTypes.Select(row => row.Value.Name));
        Assert.Equal(new[] { AirlineOffice, AgencyOffice }, full.Sales.PointsOfSale.Select(row => row.Value));
        Assert.Equal(new long[] { 9001 }, full.Sales.Customers.Select(row => row.Value));
        Assert.Equal(new[] { "TravelAgency", "Organization" }, full.Sales.CustomerTypes.Select(row => row.Value.Name));
        Assert.Equal((Thr, Ist, Mhd), (full.Travel.OriginAirports.Single().Value, full.Travel.DestinationAirports.Single().Value, full.Travel.ViaAirports.Single().Value));
        Assert.Equal(
            new[] { (9401L, Thr, Ist, "Directional"), (9402L, Mhd, Ist, "BothDirections") },
            full.Travel.RoutePairs.Select(pair => (pair.Id, pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)));
        Assert.Equal(new[] { 1, 2 }, full.Travel.MarketingAirlines.Select(row => row.Value));
        Assert.Equal(new[] { 3 }, full.Travel.OperatingAirlines.Select(row => row.Value));
        Assert.Equal(new[] { "W5112", "W5116" }, full.Travel.FlightNumbers.Select(row => row.Value));
        Assert.Equal(new long[] { 81234, 81240 }, full.Travel.Flights.Select(row => row.Value));
        Assert.Equal(new[] { 1, 2 }, full.Travel.Aircraft.Select(row => row.Value));
        Assert.Equal(new long[] { 7001 }, full.Fare.AirFares.Select(row => row.Value));
        Assert.Equal(new[] { "Public", "Private" }, full.Fare.AirFareTypes.Select(row => row.Value.Name));
        Assert.Equal(new long[] { 5, 6 }, full.Fare.FareFamilies.Select(row => row.Value));
        Assert.Equal(new[] { "Y26LT" }, full.Fare.FareBases.Select(row => row.Value));
        Assert.Equal(new[] { 2 }, full.Fare.CabinClasses.Select(row => row.Value));
        Assert.Equal(new long[] { 41, 42 }, full.Fare.Rbds.Select(row => row.Value));
        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), full.Travel.SeasonalPeriods.Select(row => (row.StartDate, row.EndDate)).Single());
        Assert.Equal(
            new[] { ("Monday", new TimeOnly(8, 0), new TimeOnly(12, 0), "Allow"), ("Friday", new TimeOnly(8, 0), new TimeOnly(12, 0), "Allow") },
            full.Travel.DayTimeRestrictions.Select(row => (row.DayOfWeek.Name, row.StartTime!.Value, row.EndTime!.Value, row.Effect.Name)));
        Assert.Empty(full.Travel.TravelDates);
        Assert.Empty(full.Travel.BlackoutPeriods);
        Assert.Equal(
            (2, 2, 1, 2, 2, 2, 1, 2, 2, 2),
            (stored.PassengerTypes.Count, stored.PointsOfSale.Count, stored.Customers.Count, stored.CustomerTypes.Count, stored.RoutePairs.Count, stored.FlightNumbers.Count,
                stored.SeasonalPeriods.Count, stored.DayTimeRestrictions.Count, stored.Rbds.Count, stored.AirFareTypes.Count));
        Assert.Equal(
            full.Passenger.PassengerTypes.Select(row => row.Id).Concat(full.Travel.DayTimeRestrictions.Select(row => row.Id)),
            stored.PassengerTypes.OrderBy(row => row.Id).Select(row => row.Id).Concat(stored.DayTimeRestrictions.OrderBy(row => row.Id).Select(row => row.Id)));

        var draft = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(DraftRule));
        var free = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FreeRule));
        var retired = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(RetiredRule));
        var suspended = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(SuspendedRule));
        var seat = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(SeatRule));

        Assert.Equal(("Draft", "Free", "Retired", "Suspended", "Active"), (draft.Status.Name, free.Disposition.Name, retired.Status.Name, suspended.Status.Name, seat.Status.Name));
        Assert.Equal(
            Enum.GetValues<DayOfWeek>().Select(day => (day.ToString(), (TimeOnly?)new TimeOnly(6, 0), (TimeOnly?)new TimeOnly(10, 0))),
            draft.Travel.DayTimeRestrictions.Select(row => (row.DayOfWeek.Name, row.StartTime, row.EndTime)));
        Assert.Empty(draft.Travel.SeasonalPeriods);
        Assert.Equal(new[] { "INF" }, free.Passenger.PassengerTypes.Select(row => row.Value.Name));
        Assert.Equal(
            new[] { ("Wednesday", (TimeOnly?)null, (TimeOnly?)null), ("Thursday", null, null), ("Friday", null, null) },
            free.Travel.DayTimeRestrictions.Select(row => (row.DayOfWeek.Name, row.StartTime, row.EndTime)));
        Assert.Empty(retired.Travel.DayTimeRestrictions);
        Assert.Equal(("Seat", "1A,1C", "E", 1), (seat.ApplicationType.Name, string.Join(',', seat.Seat!.SeatNumbers.Select(row => row.Value)), seat.Seat.SeatCharacteristics.Single().Value, seat.Travel.Aircraft.Single().Value));
        Assert.Equal(
            new[] { "9202 one-sided travel window 2026-12-01", "9204 overnight time window 22:00:00" },
            await RowsAsync(
                "SELECT CONCAT([Id], ' one-sided travel window ', [TravelFrom]) AS Value FROM [Ancillary].[AncillaryProvisions] WHERE ([TravelFrom] IS NULL AND [TravelTo] IS NOT NULL) OR ([TravelFrom] IS NOT NULL AND [TravelTo] IS NULL) " +
                "UNION ALL SELECT CONCAT([Id], ' overnight time window ', CONVERT(varchar(8), [TimeFrom])) FROM [Ancillary].[AncillaryProvisions] WHERE [TimeFrom] > [TimeTo] ORDER BY 1"));

        var pricing = await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule));

        Assert.Equal((FullRule, FullRule, 1, Eur, "EUR", "Item", "Active"), (pricing.Id, pricing.AncillaryProvisionId, pricing.Version, pricing.CurrencyId, pricing.Currency, pricing.FeeApplicationUnit!.Name, pricing.Status.Name));
        Assert.Null(pricing.PricingUnit);
        Assert.Equal((full.CreatedAt, full.ActivatedAt), (pricing.CreatedAt, pricing.ActivatedAt));
        Assert.Equal(
            new[]
            {
                (9301L, "Ancillary", (string?)null, (string?)"Extra bag", (int?)null, (int?)null, 30m),
                (9302L, "Tax", "VAT", "Value added tax", 98, Thr, 2.7m),
                (9303L, "Fee", "HDL", "Handling", null, null, 1.05m)
            },
            pricing.PriceLines.Select(line => (line.Id, line.Category.Name, line.Code, line.Name, line.CountryId, line.StationAirportId, line.Amount)));
        Assert.All(pricing.PriceLines, line => Assert.Null(line.PassengerTypeCode));
        Assert.Equal((30m, 2.7m, 1.05m, 33.75m), pricing.Rates.Select(rate => (rate.BaseAmount, rate.TaxAmount, rate.FeeAmount, rate.TotalAmount)).Single());
        Assert.Equal(
            new[]
            {
                "9201 9201 1 47 3 2 pricing-unit:",
                "9202 9202 1 70 3 1 pricing-unit:",
                "9204 9204 1 155 3 4 pricing-unit:",
                "9205 9205 1 47 3 2 pricing-unit:",
                "9206 9206 1 47 3 2 pricing-unit:",
                "9207 9207 1 47 3 1 pricing-unit:"
            },
            await RowsAsync(
                "SELECT CONCAT([Id], ' ', [AncillaryProvisionId], ' ', [Version], ' ', [CurrencyId], ' ', [FeeApplicationUnit], ' ', [Status], ' pricing-unit:', [PricingUnit]) AS Value " +
                "FROM [Ancillary].[AncillaryPricings] ORDER BY [Id]"));
        Assert.Empty(await RowsAsync(
            "SELECT CONCAT(command.[Id], '') AS Value FROM [Ancillary].[AncillaryPricings] AS command FULL JOIN [ReadModel].[AncillaryPricings] AS model ON model.[Id] = command.[Id] " +
            "WHERE command.[Id] IS NULL OR model.[Id] IS NULL OR command.[Status] <> model.[Status] OR command.[CurrencyId] <> model.[CurrencyId] OR model.[PricingUnit] IS NOT NULL"));
        Assert.Empty(await RowsAsync(
            "SELECT CONCAT(legacy.[Id], '') AS Value FROM [Ancillary].[ProvisionPriceLines] AS legacy " +
            "FULL JOIN [Ancillary].[AncillaryPricingLines] AS line ON line.[Id] = legacy.[Id] FULL JOIN [ReadModel].[AncillaryPricingLines] AS model ON model.[Id] = legacy.[Id] " +
            "WHERE legacy.[Id] IS NULL OR line.[Id] IS NULL OR model.[Id] IS NULL OR line.[Amount] <> legacy.[UnitAmount] OR model.[Amount] <> legacy.[UnitAmount] " +
            "OR line.[Category] <> legacy.[Category] OR line.[AncillaryPricingId] <> legacy.[AncillaryProvisionId] OR ISNULL(line.[Code], '') <> ISNULL(legacy.[Code], '')"));
        Assert.Equal(
            new[] { "155 22.00", "47 65.74", "70 15000000.00" },
            await RowsAsync(
                "SELECT CONCAT(pricing.[CurrencyId], ' ', SUM(line.[Amount])) AS Value FROM [Ancillary].[AncillaryPricingLines] AS line " +
                "JOIN [Ancillary].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId] GROUP BY pricing.[CurrencyId] ORDER BY 1"));
        Assert.Equal(
            new[] { ("Retired", "USD"), ("Active", "EUR") },
            new[]
            {
                await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(RetiredRule)),
                await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(SuspendedRule))
            }.Select(row => (row.Status.Name, row.Currency!)));
        Assert.Equal(retired.RetiredAt, (await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(RetiredRule))).RetiredAt);
        Assert.Empty((await RequestAsync(scope => scope.GetPricingsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = FreeRule }))).Results);

        var definition = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(BagDefinition));
        var supplier = await RequestAsync(scope => scope.GetSupplierById.ExecuteAsync(9001));

        Assert.Equal(("XBAG_PIECE_23KG", "0CC", "Industry", "BG", "B1", "EmdAssociated", "Ssr", "XBAG", "Active"), (definition.ServiceDefinitionRef, definition.ServiceSubCode, definition.SubCodeSource.Name, definition.GroupCode, definition.Description1Code, definition.DocumentType.Name, definition.BookingMethod.Name, definition.BookingSsrCode, definition.Status.Name));
        Assert.Null(definition.PricingUnit);
        Assert.Equal(("Dot Air", "Local", "Active"), (supplier.Name, supplier.FulfillmentKind.Name, supplier.Status.Name));
        Assert.Equal(
            new[] { "Ancillary.9101:", "Ancillary.9102:", "ReadModel.9101:", "ReadModel.9102:" },
            await RowsAsync(
                "SELECT CONCAT('Ancillary.', [Id], ':', [PricingUnit]) AS Value FROM [Ancillary].[AncillaryServiceDefinitions] " +
                "UNION ALL SELECT CONCAT('ReadModel.', [Id], ':', [PricingUnit]) FROM [ReadModel].[AncillaryServiceDefinitions] ORDER BY 1"));
    }

    [Fact]
    public async Task V12_C02_C03_legacy_rows_stay_unmapped_until_an_explicit_pricing_unit_is_assigned_and_then_work_as_v12_rows()
    {
        await RefusedAsync(16210, 409, scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(BagDefinition)));
        await RefusedAsync(16210, 409, scope => scope.DefinePricing.DefineAsync(Pricing(SuspendedRule, Eur, Base(11m))));
        await RefusedAsync(16210, 409, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(DraftRule)));
        await RefusedAsync(16210, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));
        await RefusedAsync(16210, 409, scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(SimDefinition)));
        await RefusedAsync(16210, 409, scope => scope.DefineServiceDefinition.DefineAsync(
            FirstExcessBagDefinition(7, 9001, "XBAG_PIECE_23KG")));

        Assert.Equal("Active", (await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule))).Status.Name);
        Assert.Equal("Active", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FullRule))).Status.Name);

        var assigned = await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(BagDefinition, PricingUnit.PerPiece)));

        Assert.Equal((BagDefinition, PricingUnit.PerPiece, ServiceDefinitionStatus.Active), (assigned.Id, assigned.PricingUnit!.Value, assigned.Status));
        Assert.Equal(
            new[]
            {
                "Ancillary 9201 6", "Ancillary 9202 6", "Ancillary 9204 6", "Ancillary 9205 6", "Ancillary 9206 6", "Ancillary 9207 ",
                "ReadModel 9201 6", "ReadModel 9202 6", "ReadModel 9204 6", "ReadModel 9205 6", "ReadModel 9206 6", "ReadModel 9207 "
            },
            await RowsAsync(
                "SELECT CONCAT('Ancillary ', [Id], ' ', [PricingUnit]) AS Value FROM [Ancillary].[AncillaryPricings] " +
                "UNION ALL SELECT CONCAT('ReadModel ', [Id], ' ', [PricingUnit]) FROM [ReadModel].[AncillaryPricings] ORDER BY 1"));
        Assert.Equal("PerPiece", (await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(BagDefinition))).PricingUnit!.Name);
        await RefusedAsync(16211, 409, scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(BagDefinition, PricingUnit.PerPiece)));
        await RefusedAsync(16209, 409, scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(BagDefinition, PricingUnit.PerKilogram)));

        var published = await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));

        Assert.Equal(ProvisionStatus.Active, published.Status);
        Assert.Equal(("Active", "IRR", 15000000m), await RequestAsync(async scope =>
        {
            var pricing = await scope.GetPricingById.ExecuteAsync(DraftRule);

            return (pricing.Status.Name, pricing.Currency!, pricing.Rates.Single().TotalAmount);
        }));

        var revision = await RequestAsync(scope => scope.RevisePricing.ReviseAsync(new TestPricingLifecycleCommand(FullRule)));

        Assert.Equal((2, PricingUnit.PerPiece, PricingStatus.Draft), (revision.Version, revision.PricingUnit!.Value, revision.Status));

        await RequestAsync(scope => scope.ChangePricing.ChangeAsync(
            Change(revision.Id, Pricing(FullRule, Eur, Base(32m, name: "Extra bag"), Tax("VAT", 2.88m, countryId: 98, stationAirportId: Thr), Fee("HDL", 1.05m)))));

        var switched = await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(FullRule, revision.Id, FullRule)));
        var original = await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule));

        Assert.Equal((revision.Id, PricingStatus.Active), (switched.Id, switched.Status));
        Assert.Equal(("Retired", "PerPiece", 33.75m), (original.Status.Name, original.PricingUnit!.Name, original.Rates.Single().TotalAmount));
        Assert.Equal(new long[] { 9301, 9302, 9303 }, original.PriceLines.Select(line => line.Id));

        await RefusedAsync(16502, 422, scope => scope.RevisePricing.ReviseAsync(new TestPricingLifecycleCommand(SuspendedRule)));

        var corrected = await RequestAsync(scope => scope.DefinePricing.DefineAsync(
            Pricing(SuspendedRule, Eur, Base(9.99m, name: "Extra bag"), Fee("HDL", 1m))));

        await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(SuspendedRule, corrected.Id, SuspendedRule)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(SuspendedRule)))).Status);
        Assert.Equal(
            new[] { ("Retired", 1, 10.99m), ("Active", 2, 10.99m) },
            (await RequestAsync(async scope =>
            {
                var first = await scope.GetPricingById.ExecuteAsync(SuspendedRule);
                var second = await scope.GetPricingById.ExecuteAsync(corrected.Id);

                return new[] { first, second };
            })).Select(row => (row.Status.Name, row.Version, row.Rates.Single().TotalAmount)));

        var sim = await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(SimDefinition, PricingUnit.PerItem)));

        Assert.Equal((PricingUnit.PerItem, ServiceDefinitionStatus.Draft), (sim.PricingUnit!.Value, sim.Status));
        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(SimDefinition)));
        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(SimRule, SimRule)))).Status);
        Assert.Equal(
            ("Active", "PerItem", 9m),
            await RequestAsync(async scope =>
            {
                var pricing = await scope.GetPricingById.ExecuteAsync(SimRule);

                return (pricing.Status.Name, pricing.PricingUnit!.Name, pricing.Rates.Single().TotalAmount);
            }));

        await RefusedAsync(16303, 409, scope => scope.AddTravelDate.AddAsync(new TestTravelDateRowCommand(FreeRule, 0, new DateOnly(2027, 1, 1))));
    }
}
