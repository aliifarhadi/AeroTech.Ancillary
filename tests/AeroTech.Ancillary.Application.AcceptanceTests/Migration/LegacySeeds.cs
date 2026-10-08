namespace AeroTech.Ancillary.Application.AcceptanceTests.Migration;

public static class LegacySeeds
{
    public const string V11Command = "20261007202914_V11Phase1CommercialAuthoring";
    public const string V11Query = "20261007202918_V11Phase1CommercialAuthoringQuery";
    public const string V12Command = "20261008103942_V12Phase1NormalizedProvisionAndPricing";
    public const string V12Query = "20261008103947_V12Phase1NormalizedProvisionAndPricingQuery";
    public const string V121Command = "20261008135403_V121Phase1RuleGroups";
    public const string V121Query = "20261008135407_V121Phase1RuleGroupsQuery";

    public const long AirlineOffice = 9200000000000001;
    public const long AgencyOffice = 1551571720488353792;
    public const long BagDefinition = 9101;
    public const long SimDefinition = 9102;
    public const long FullRule = 9201;
    public const long DraftRule = 9202;
    public const long FreeRule = 9203;
    public const long RetiredRule = 9204;
    public const long SuspendedRule = 9205;
    public const long SeatRule = 9206;
    public const long SimRule = 9207;

    public static readonly (string Command, string ReadModel)[] RuleRowTables =
    [
        ("ProvisionPassengerTypes", "AncillaryProvisionPassengerTypes"),
        ("ProvisionEligibleAgeBands", "AncillaryProvisionEligibleAgeBands"),
        ("ProvisionPointsOfSale", "AncillaryProvisionPointsOfSale"),
        ("ProvisionCustomers", "AncillaryProvisionCustomers"),
        ("ProvisionCustomerTypes", "AncillaryProvisionCustomerTypes"),
        ("ProvisionOriginAirports", "AncillaryProvisionOriginAirports"),
        ("ProvisionDestinationAirports", "AncillaryProvisionDestinationAirports"),
        ("ProvisionViaAirports", "AncillaryProvisionViaAirports"),
        ("ProvisionRoutePairs", "AncillaryProvisionRoutePairs"),
        ("ProvisionServiceLocations", "AncillaryProvisionServiceLocations"),
        ("ProvisionCoverageCountries", "AncillaryProvisionCoverageCountries"),
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
        ("ProvisionPermittedTravelPeriods", "AncillaryProvisionPermittedTravelPeriods"),
        ("ProvisionBlackoutPeriods", "AncillaryProvisionBlackoutPeriods"),
        ("ProvisionDayTimeWindows", "AncillaryProvisionDayTimeWindows"),
        ("ProvisionSeatNumbers", "AncillaryProvisionSeatNumbers"),
        ("ProvisionSeatCharacteristics", "AncillaryProvisionSeatCharacteristics")
    ];

    public static readonly (string RuleTable, string ReadColumn)[] RuleGroups =
    [
        ("ProvisionPassengerEligibilityRules", "PassengerEligibilityRuleId"),
        ("ProvisionSalesRestrictionsRules", "SalesRestrictionsRuleId"),
        ("ProvisionGeographyRules", "GeographyRuleId"),
        ("ProvisionFlightApplicationRules", "FlightApplicationRuleId"),
        ("ProvisionFareApplicationRules", "FareApplicationRuleId"),
        ("ProvisionTravelDateRules", "TravelDateRuleId"),
        ("ProvisionDayTimeApplicationRules", "DayTimeApplicationRuleId"),
        ("ProvisionAdvancePurchaseRules", "AdvancePurchaseRuleId"),
        ("ProvisionBaggageApplicationRules", "BaggageApplicationRuleId"),
        ("ProvisionSeatApplicationRules", "SeatApplicationRuleId")
    ];

    public const string V11 =
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

    public const string V12DatesAndTimes =
        """
        DECLARE @now datetimeoffset = '2026-09-05T08:00:00+00:00';

        INSERT INTO [Ancillary].[ProvisionTravelDates] ([Id], [AncillaryProvisionId], [TravelDate], [LastUpdateTime], [LastUpdatedBy])
        SELECT 700000 + numbers.[Value], 9205, DATEADD(DAY, numbers.[Value] - 1, CAST('2027-01-01' AS date)), @now, 42
        FROM (SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS [Value] FROM sys.all_objects AS a CROSS JOIN sys.all_objects AS b) AS numbers;

        INSERT INTO [Ancillary].[ProvisionTravelDates] ([Id], [AncillaryProvisionId], [TravelDate], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (710001, 9201, '2026-12-24', @now, 42),
            (710002, 9201, '2026-12-25', @now, 42),
            (710003, 9201, '2027-01-05', @now, 42),
            (720002, 9203, '2027-07-15', @now, 42),
            (730001, 9204, '2027-02-01', @now, 42),
            (730002, 9204, '2027-02-02', @now, 42),
            (730003, 9204, '2027-02-10', @now, 42);

        INSERT INTO [Ancillary].[ProvisionSeasonalPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (720001, 9203, '2027-06-01', '2027-06-30', @now, 42),
            (740001, 9206, '2027-03-01', '2027-03-31', @now, 42),
            (740002, 9206, '2027-03-20', '2027-04-10', @now, 42),
            (740003, 9206, '2027-04-11', '2027-04-30', @now, 42),
            (740004, 9206, '2027-06-01', '2027-06-30', @now, 42);

        INSERT INTO [Ancillary].[ProvisionBlackoutPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate], [LastUpdateTime], [LastUpdatedBy])
        VALUES (750001, 9207, '2027-12-24', '2027-12-26', @now, 42);

        INSERT INTO [Ancillary].[ProvisionDayTimeRestrictions] ([Id], [AncillaryProvisionId], [DayOfWeek], [StartTime], [EndTime], [Effect], [LastUpdateTime], [LastUpdatedBy])
        VALUES
            (750002, 9207, 5, NULL, NULL, 2, @now, 42),
            (750003, 9207, 1, '09:00', '10:00', 2, @now, 42),
            (760001, 9202, 6, '22:00', '02:00', 1, @now, 42);

        INSERT INTO [ReadModel].[AncillaryProvisionTravelDates] ([Id], [AncillaryProvisionId], [TravelDate])
        SELECT [Id], [AncillaryProvisionId], [TravelDate] FROM [Ancillary].[ProvisionTravelDates] WHERE [Id] BETWEEN 700000 AND 799999;

        INSERT INTO [ReadModel].[AncillaryProvisionSeasonalPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate])
        SELECT [Id], [AncillaryProvisionId], [StartDate], [EndDate] FROM [Ancillary].[ProvisionSeasonalPeriods] WHERE [Id] BETWEEN 700000 AND 799999;

        INSERT INTO [ReadModel].[AncillaryProvisionBlackoutPeriods] ([Id], [AncillaryProvisionId], [StartDate], [EndDate])
        SELECT [Id], [AncillaryProvisionId], [StartDate], [EndDate] FROM [Ancillary].[ProvisionBlackoutPeriods] WHERE [Id] BETWEEN 700000 AND 799999;

        INSERT INTO [ReadModel].[AncillaryProvisionDayTimeRestrictions] ([Id], [AncillaryProvisionId], [DayOfWeek], [StartTime], [EndTime], [Effect])
        SELECT [Id], [AncillaryProvisionId], [DayOfWeek], [StartTime], [EndTime], [Effect] FROM [Ancillary].[ProvisionDayTimeRestrictions] WHERE [Id] BETWEEN 700000 AND 799999;
        """;
}
