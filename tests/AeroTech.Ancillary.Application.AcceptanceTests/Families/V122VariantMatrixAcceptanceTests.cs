using System.Collections.Concurrent;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

public sealed class VariantMatrixCache
{
    private readonly ConcurrentDictionary<string, Lazy<Task<object>>> _stages = new();

    public async Task<TStage> GetAsync<TStage>(string key, Func<Task<TStage>> build)
        where TStage : class
        => (TStage)await _stages.GetOrAdd(key, _ => new Lazy<Task<object>>(async () => (object)await build())).Value;
}

[Collection(DatabaseCollection.Name)]
public class V122VariantMatrixAcceptanceTests : IClassFixture<VariantMatrixCache>
{
    private const long Flight = 81234;
    private const int Usd = 155;
    private const string FlightFlowKey = "FlightFlow";

    private static readonly DateTimeOffset At = Utc(10, 15);

    private sealed record Defined(
        int AirlineId,
        long SupplierId,
        TestDefineServiceDefinitionCommand Command,
        long DefinitionId,
        BackofficeServiceDefinitionDto Draft,
        BackofficeServiceDefinitionDto Edited,
        int[] Refusals);

    private sealed record Ruled(Defined Definition, TestDefineProvisionCommand Command, long ProvisionId, BackofficeProvisionDto Draft, int TwoPointsOfSale, int NoPointOfSale);

    private sealed record Priced(Ruled Rule, BackofficeProvisionDto Provision, BackofficePricingDto? Pricing, int PricingRefusal, int ActivePricings, int ForeignQuoteAuthority);

    private sealed record Stocked(
        int OfflineRefusal,
        InventoryConfigurationSnapshotDto Before,
        InventoryConfigurationSnapshotDto Snapshot,
        BackofficeInventoryPolicyDto Policy,
        string SourceRows);

    private sealed record Read(
        BackofficeServiceDefinitionDto Detail,
        ServiceDefinitionPaginatedRowDto Listed,
        int ActiveEdit,
        BackofficeServiceDefinitionDto Revision,
        int VariantDrift,
        string[] SpecificationRows,
        string ReadModelRow,
        BackofficeProvisionDto Provision);

    private sealed record Negative(
        int Code,
        Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand>? Definition = null,
        Func<TestDefineProvisionCommand, TestDefineProvisionCommand>? Rule = null,
        bool AtActivation = false);

    private static readonly IReadOnlyDictionary<string, Negative> Negatives = new Dictionary<string, Negative>
    {
        ["A01"] = new(16302, Rule: rule => rule with { BaggageApplication = Baggage(23m, 3, 2) }),
        ["A02"] = new(16202, OfBaggage(baggage => baggage with { AllowanceConcept = BaggageAllowanceConcept.Piece })),
        ["A03"] = new(16202, OfBaggage(baggage => baggage with { WeightFromExclusiveKg = 32m, WeightToInclusiveKg = 23m })),
        ["A04"] = new(16202, OfBaggage(baggage => baggage with { MaxSize = new DimensionsCmInput(0m, 80m, 60m) })),
        ["A05"] = new(16202, OfBaggage(baggage => baggage with { PackageWeightKg = 5m })),
        ["A06"] = new(16202, OfBaggage(baggage => baggage with { EquipmentKind = "bike!" })),
        ["A07"] = new(16321, Rule: rule => Unavailable(rule) with { Quantity = new(AncillaryQuantityUnit.Each, 1, 2) }, AtActivation: true),
        ["A08"] = new(16202, OfSpecification(specification => specification with { Seat = specification.Seat! with { SeatCharacteristicCodes = [] } })),
        ["A09"] = new(16202, command => command with { Routing = DocumentRouting.Emd, Document = new(AncillaryDocumentType.EmdAssociated, "A", command.ServiceSubCode) }),
        ["A10"] = new(16202, command => command with { Routing = DocumentRouting.Emd, Document = new(AncillaryDocumentType.EmdAssociated, "A", command.ServiceSubCode) }),
        ["A11"] = new(16202, OfSpecification(specification => specification with { Meal = specification.Meal! with { MealCode = "KSML" } })),
        ["A12"] = new(16202, OfSpecification(specification => specification with { Meal = specification.Meal! with { MenuItemRef = null } })),
        ["A13"] = new(16202, command => command with { Booking = command.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate } }),
        ["A14"] = new(16202, OfSpecification(specification => specification with
        {
            Pet = specification.Pet! with { AllowedHoldAnimalSizeBrackets = [new("MEDIUM", 8m, 40m), new("LARGE", 32m, 75m)] }
        })),
        ["A15"] = new(16202, command => command with { Booking = command.Booking with { SsrCode = "BLND" } }),
        ["A16"] = new(16202, OfAssisted(assisted => assisted with { DisabilityAssistance = assisted.DisabilityAssistance! with { AllowedSsrCodes = ["WCHR"] } })),
        ["A17"] = new(16202, command => command with { Booking = command.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate } }),
        ["A18"] = new(16202, OfAssisted(assisted => assisted with { Bassinet = assisted.Bassinet! with { RequiresInfantAndGuardian = false } })),
        ["A19"] = new(16202, OfAssisted(assisted => assisted with { UnaccompaniedMinor = assisted.UnaccompaniedMinor! with { MinAgeYears = 12, MaxAgeYearsExclusive = 5 } })),
        ["A20"] = new(16202, OfAirport(airport => airport with { IanaTimeZone = null })),
        ["A21"] = new(16202, OfAirport(airport => airport with { IanaTimeZone = "Mars/Phobos" })),
        ["A22"] = new(16218, OfAirport(airport => airport with { AirportId = 99 }), AtActivation: true),
        ["A23"] = new(16218, OfSpecification(specification => specification with { Priority = specification.Priority! with { AirportIds = [99] } }), AtActivation: true),
        ["A24"] = new(
            16323,
            OfSpecification(specification => specification with { Connectivity = specification.Connectivity! with { DeliveryStage = PurchaseStage.OnBoard } }),
            Unavailable,
            true)
    };

    private readonly TestDatabase _database;
    private readonly VariantMatrixCache _cache;
    private readonly FixedClock _clock = new();

    public V122VariantMatrixAcceptanceTests(TestDatabase database, VariantMatrixCache cache)
    {
        _database = database;
        _cache = cache;
    }

    public static IEnumerable<object[]> Variants => V122Catalog.Cases.Select(variant => new object[] { variant.Code });

    private static Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand> OfSpecification(Func<ServiceSpecificationInput, ServiceSpecificationInput> change)
        => command => command with { TypedSpecification = change(command.TypedSpecification!) };

    private static Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand> OfBaggage(Func<BaggageSpecificationInput, BaggageSpecificationInput> change)
        => OfSpecification(specification => specification with { Baggage = change(specification.Baggage!) });

    private static Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand> OfAssisted(
        Func<AssistedTravelSpecificationInput, AssistedTravelSpecificationInput> change)
        => OfSpecification(specification => specification with { AssistedTravel = change(specification.AssistedTravel!) });

    private static Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand> OfAirport(
        Func<AirportServiceSpecificationInput, AirportServiceSpecificationInput> change)
        => OfSpecification(specification => specification with { AirportService = change(specification.AirportService!) });

    private static TestDefineProvisionCommand Unavailable(TestDefineProvisionCommand rule)
        => rule with
        {
            Outcome = new(CommercialDisposition.NotAvailable, false, false),
            Origin = PriceOrigin.NotAvailable,
            QuoteProviderKey = null
        };

    private static async Task<int> CodeAsync(Func<Task> request)
    {
        try
        {
            await request();

            return 0;
        }
        catch (BusinessException exception)
        {
            return exception.Code;
        }
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task<int> RefusalAsync<TResult>(Func<AncillaryScope, Task<TResult>> request) => CodeAsync(() => RequestAsync(request));

    private async Task<(int AirlineId, long SupplierId)> OperatorAsync(VariantCase variant)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await RequestAsync(scope => scope.RegisterSupplier.RegisterAsync(
            V122Catalog.NeedsExternalSupplier(variant) ? M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider) : M1Commands.LocalSupplier(airlineId)));

        return (airlineId, supplierId.Id);
    }

    private Task<Defined> DefinedAsync(string code) => _cache.GetAsync($"{code}-D", async () =>
    {
        var variant = V122Catalog.Case(code);
        var (airlineId, supplierId) = await OperatorAsync(variant);
        var command = V122Catalog.Define(variant, airlineId, supplierId);
        var foreign = V122Catalog.Specification(variant.Profile == AncillaryProfile.Priority ? AncillaryVariant.OnboardWifi : AncillaryVariant.PriorityBoardingCheckin);
        var doubled = foreign.Priority is null ? variant.Specification with { Connectivity = foreign.Connectivity } : variant.Specification with { Priority = foreign.Priority };
        int[] refusals =
        [
            await RefusalAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with { TypedSpecification = foreign })),
            await RefusalAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with { TypedSpecification = doubled })),
            await RefusalAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with { Variant = "A99" }))
        ];
        var definitionId = (await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command))).Id;
        var draft = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(definitionId));

        await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(Change(definitionId, command with { CommercialName = variant.Name + " (edited)" })));

        var edited = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(definitionId));

        return new Defined(airlineId, supplierId, command, definitionId, draft, edited, refusals);
    });

    private Task<Ruled> RuledAsync(string code) => _cache.GetAsync($"{code}-R", async () =>
    {
        var variant = V122Catalog.Case(code);
        var definition = await DefinedAsync(code);

        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.DefinitionId)));

        var command = V122Catalog.Rule(variant, definition.DefinitionId);
        var provisionId = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(command))).Id;
        var draft = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));
        var twoPointsOfSale = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(Unavailable(command) with
        {
            Sequence = 80,
            SalesRestrictions = new(AllowedPointOfSaleIds: [V122Catalog.PointOfSale, V122Catalog.OtherPointOfSale])
        }))).Id;
        var noPointOfSale = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(Unavailable(command) with { Sequence = 81, SalesRestrictions = null }))).Id;

        return new Ruled(
            definition,
            command,
            provisionId,
            draft,
            await RefusalAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(twoPointsOfSale))),
            await RefusalAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(noPointOfSale))));
    });

    private Task<Priced> PricedAsync(string code) => _cache.GetAsync($"{code}-P", async () =>
    {
        var variant = V122Catalog.Case(code);
        var rule = await RuledAsync(code);
        long? pricingId = null;
        var pricingRefusal = 0;
        var foreignQuoteAuthority = 0;

        if (variant.PriceOrigin == PriceOrigin.Filed)
        {
            IReadOnlyList<PricingRateInput> rates =
            [
                new(null, null, null, new MoneyInput(100m, Eur),
                [
                    new(AncillaryPriceLineCategory.Tax, "VAT", null, null, null, new MoneyInput(9m, Eur), null, null, TaxTreatment.AddedToBase),
                    new(AncillaryPriceLineCategory.Tax, "INC", null, null, null, new MoneyInput(5m, Eur), null, true, TaxTreatment.IncludedInBase),
                    new(AncillaryPriceLineCategory.Fee, "TKT", null, null, null, new MoneyInput(7m, Eur), FeeApplicationUnit.Ticket, null)
                ]),
                new(null, null, null, new MoneyInput(120m, Usd), null)
            ];

            pricingId = (await RequestAsync(scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(rule.ProvisionId, rates)))).Id;
            await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(rule.ProvisionId, pricingId.Value)));
        }
        else
        {
            if (variant.PriceOrigin == PriceOrigin.ExternalQuote)
            {
                var foreign = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(rule.Command with { Sequence = 20, QuoteProviderKey = "OtherPartner" }))).Id;

                foreignQuoteAuthority = await RefusalAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(foreign)));
            }

            await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(rule.ProvisionId)));
            pricingRefusal = await RefusalAsync(scope => scope.DefinePricing.DefineAsync(
                new TestDefinePricingRatesCommand(rule.ProvisionId, [new(null, null, null, new MoneyInput(1m, Eur), null)])));
        }

        return new Priced(
            rule,
            await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(rule.ProvisionId)),
            pricingId is { } id ? await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(id)) : null,
            pricingRefusal,
            await RequestAsync(scope => scope.Command.AncillaryPricings.AsNoTracking().CountAsync(row => row.AncillaryProvisionId == rule.ProvisionId)),
            foreignQuoteAuthority);
    });

    private static PassengerUsageLimitInput LimitOf(VariantCase variant)
        => variant.Code == AncillaryVariant.ExtraWeightPackage
            ? new(PassengerUsageLimitScope.PerPortion, 20, variant.Reference, UsageConsumptionUnit.Kilogram, 10m)
            : new(
                variant.CoverageScope == ServiceCoverageScope.Portion ? PassengerUsageLimitScope.PerPortion : PassengerUsageLimitScope.PerFlightOccurrence,
                variant.MaxQuantity,
                variant.Reference);

    private Task<Stocked> StockedAsync(string code) => _cache.GetAsync($"{code}-I", async () =>
    {
        var variant = V122Catalog.Case(code);
        var price = await PricedAsync(code);
        var definition = price.Rule.Definition;
        var harness = new InventoryHarness(_database, _clock);
        var fixture = new InventoryFixture(definition.AirlineId);
        var command = new TestDefineInventoryPolicyCommand(
            definition.AirlineId,
            variant.Reference,
            definition.DefinitionId,
            variant.Authority,
            ProviderKey: variant.Authority switch
            {
                InventoryAuthority.Supplier => V122Catalog.QuoteProvider,
                InventoryAuthority.FlightFlow => FlightFlowKey,
                _ => null
            },
            PassengerUsageLimits: [LimitOf(variant)]);
        var draft = await harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));
        var offline = await CodeAsync(() => harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1))));
        var before = await harness.SnapshotAsync(fixture, variant.Reference, Flight, At);

        fixture.CountingFamilies = [variant.Reference];
        fixture.FlightFlowProviderKeys = [FlightFlowKey];
        await harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)));

        return new Stocked(
            offline,
            before,
            await harness.SnapshotAsync(fixture, variant.Reference, Flight, At),
            await harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(draft.Id)),
            (await harness.RowsAsync($"""
                SELECT CONCAT(
                    (SELECT COUNT(*) FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {definition.AirlineId}), '|',
                    (SELECT COUNT(*) FROM Ancillary.FlightWeightInventories WHERE OwnerAirlineId = {definition.AirlineId}), '|',
                    (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {definition.AirlineId})) AS Value
                """)).Single());
    });

    private Task<Read> ReadAsync(string code) => _cache.GetAsync($"{code}-Q", async () =>
    {
        var variant = V122Catalog.Case(code);
        var price = await PricedAsync(code);
        var definition = price.Rule.Definition;
        var definitionId = definition.DefinitionId;
        var harness = new InventoryHarness(_database, _clock);
        var detail = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(definitionId));
        var listed = (await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = definition.AirlineId })))
            .Results.Single(row => row.Id == definitionId.ToString());
        var activeEdit = await RefusalAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(Change(definitionId, definition.Command with { CommercialName = "Renamed" })));
        var revisionId = (await RequestAsync(scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(definitionId)))).Id;
        var revision = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(revisionId));
        var other = V122Catalog.Cases.First(candidate =>
            candidate.Code != code
            && AncillaryVariant.Find(candidate.Code)!.PricingUnits.Contains(variant.PricingUnit)
            && AncillaryVariant.Find(candidate.Code)!.ServiceDateBases.Contains(variant.ServiceDateBasis));
        var drift = await RefusalAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(Change(
            revisionId,
            V122Catalog.Define(other, definition.AirlineId, definition.SupplierId) with
            {
                ServiceDefinitionRef = variant.Reference,
                PricingUnit = variant.PricingUnit,
                ServiceDateBasis = variant.ServiceDateBasis
            })));
        var specificationRows = await harness.RowsAsync($"""
            SELECT CONCAT(counted.Name, '=', counted.Total) AS Value
            FROM (
                SELECT 'Baggage' AS Name, COUNT(*) AS Total FROM Ancillary.BaggageSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Seat', COUNT(*) FROM Ancillary.SeatSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Upgrade', COUNT(*) FROM Ancillary.UpgradeSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Meal', COUNT(*) FROM Ancillary.MealSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Pet', COUNT(*) FROM Ancillary.PetSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'AssistedTravel', COUNT(*) FROM Ancillary.AssistedTravelSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'AirportService', COUNT(*) FROM Ancillary.AirportServiceSpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Priority', COUNT(*) FROM Ancillary.PrioritySpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})
                UNION ALL SELECT 'Connectivity', COUNT(*) FROM Ancillary.ConnectivitySpecifications WHERE AncillaryServiceDefinitionId IN ({definitionId}, {revisionId})) AS counted
            WHERE counted.Total > 0
            """);
        var readModelRow = (await harness.RowsAsync(
            $"SELECT CONCAT(Profile, '|', VariantCode, '|', DocumentRouting, '|', Status) AS Value FROM ReadModel.AncillaryServiceDefinitions WHERE Id = {definitionId}")).Single();

        return new Read(
            detail,
            listed,
            activeEdit,
            revision,
            drift,
            specificationRows.ToArray(),
            readModelRow,
            await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(price.Rule.ProvisionId)));
    });

    private static void AssertTyped(VariantCase variant, BackofficeServiceDefinitionDto definition)
    {
        Assert.Equal(
            (variant.Profile.ToString(), variant.Code, AncillaryVariant.Find(variant.Code)!.Name, variant.Routing.ToString()),
            (definition.Profile!.Name, definition.VariantCode, definition.VariantName, definition.DocumentRouting!.Name));
        Assert.Equal(new[] { variant.Profile.ToString() }, SpecText.Members(definition.Specification));
        Assert.Equal(SpecText.Of(variant.Specification), SpecText.Of(definition.Specification));
        Assert.Equal(
            (variant.PricingUnit.ToString(), variant.ServiceDateBasis.ToString(), variant.Document.Type.ToString(), variant.Booking.Method.ToString(), variant.Booking.SsrCode),
            (definition.PricingUnit!.Name, definition.ServiceDateBasis!.Name, definition.DocumentType.Name, definition.BookingMethod.Name, definition.BookingSsrCode));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_D_a_draft_is_defined_and_edited_with_exactly_its_typed_specification_and_foreign_or_unknown_shapes_are_refused(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var defined = await DefinedAsync(variant);

        Assert.Equal(("Draft", authored.Name, 1), (defined.Draft.Status.Name, defined.Draft.CommercialName, defined.Draft.Version));
        AssertTyped(authored, defined.Draft);
        Assert.Equal(("Draft", authored.Name + " (edited)"), (defined.Edited.Status.Name, defined.Edited.CommercialName));
        AssertTyped(authored, defined.Edited);
        Assert.Equal(new[] { 16216, 16216, 16202 }, defined.Refusals);
        Assert.NotEqual("{}", SpecText.Of(defined.Draft.Specification));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_R_the_provision_keeps_one_point_of_sale_its_scope_quantity_outcome_and_typed_rules_and_another_point_of_sale_count_is_refused(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var ruled = await RuledAsync(variant);
        var draft = ruled.Draft;

        Assert.Equal(
            ("Draft", authored.CoverageScope.ToString(), authored.QuantityUnit.ToString(), 1, authored.MaxQuantity, authored.Disposition.ToString(), authored.ApplicationType.ToString(), "Both"),
            (draft.Status.Name, draft.CoverageScope.Name, draft.QuantityUnit.Name, draft.MinQuantity, draft.MaxQuantity, draft.Disposition.Name, draft.ApplicationType.Name, draft.PurchaseStage.Name));
        Assert.Equal(new[] { V122Catalog.PointOfSale }, draft.SalesRestrictions!.AllowedPointsOfSale.Select(row => row.Value));
        Assert.Equal(new[] { "ADT", "CHD" }, draft.PassengerEligibility!.AllowedPassengerTypes.Select(row => row.Value.Name));
        Assert.Equal((24, "Hours"), (draft.AdvancePurchase!.MinimumPeriod, draft.AdvancePurchase.Unit.Name));
        Assert.Equal((new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31)), draft.TravelDate!.PermittedPeriods.Select(row => (row.StartDate, row.EndDate)).Single());
        Assert.Equal(authored.Profile == AncillaryProfile.Baggage, draft.BaggageApplication is not null);
        Assert.Equal(authored.Profile == AncillaryProfile.Seat, draft.SeatApplication is not null);
        Assert.Equal(
            (ruled.Command.PetRule?.MaxCombinedKgOverride, ruled.Command.PetRule?.MinAnimalAgeWeeksOverride, ruled.Command.PetRule?.CountryExceptionCode),
            (draft.PetRule?.MaxCombinedKgOverride, draft.PetRule?.MinAnimalAgeWeeksOverride, draft.PetRule?.CountryExceptionCode));
        Assert.Equal(
            (ruled.Command.AssistedTravelRule?.MinimumLeadTimeMinutes, ruled.Command.AssistedTravelRule?.ConnectionPolicy?.ToString(), ruled.Command.AssistedTravelRule?.MedicalApprovalRequired),
            (draft.AssistedTravelRule?.MinimumLeadTimeMinutes, draft.AssistedTravelRule?.ConnectionPolicy?.Name, draft.AssistedTravelRule?.MedicalApprovalRequired));
        Assert.Equal(
            (ruled.Command.AirportServiceRule?.TerminalRef, ruled.Command.AirportServiceRule?.Direction?.ToString(), ruled.Command.AirportServiceRule?.FacilityId, ruled.Command.AirportServiceRule?.MaxGuestsPerPrimary),
            (draft.AirportServiceRule?.TerminalRef, draft.AirportServiceRule?.Direction?.Name, draft.AirportServiceRule?.FacilityId, draft.AirportServiceRule?.MaxGuestsPerPrimary));
        Assert.Equal((16319, 16319), (ruled.TwoPointsOfSale, ruled.NoPointOfSale));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_P_the_price_origin_is_published_as_filed_money_an_external_quote_or_no_price_and_never_as_a_fabricated_amount(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var priced = await PricedAsync(variant);

        Assert.Equal(("Active", authored.PriceOrigin.ToString(), authored.Disposition.ToString()), (priced.Provision.Status.Name, priced.Provision.PriceOrigin.Name, priced.Provision.Disposition.Name));

        if (authored.PriceOrigin == PriceOrigin.Filed)
        {
            var pricing = priced.Pricing!;
            var euro = pricing.Rates.Single(rate => rate.BasePrice.CurrencyId == Eur);
            var dollar = pricing.Rates.Single(rate => rate.BasePrice.CurrencyId == Usd);

            Assert.Equal(("Active", authored.PricingUnit.ToString(), 1), (pricing.Status.Name, pricing.PricingUnit!.Name, priced.ActivePricings));
            Assert.Equal((100m, 109m, 5m, false), (euro.BasePrice.Amount, euro.UnitTotal.Amount, euro.IncludedTaxes.Amount, euro.IsUnitTotalComplete));
            Assert.Equal(new[] { "TKT" }, euro.UnappliedFees.Select(fee => fee.Code));
            Assert.Equal(
                new[] { "INC:IncludedInBase", "TKT:", "VAT:AddedToBase" },
                euro.Components.Select(component => $"{component.Code}:{component.TaxTreatment?.Name}").OrderBy(text => text, StringComparer.Ordinal));
            Assert.Equal((120m, 120m, 0m, true), (dollar.BasePrice.Amount, dollar.UnitTotal.Amount, dollar.IncludedTaxes.Amount, dollar.IsUnitTotalComplete));
            Assert.Null(priced.Provision.QuoteProviderKey);

            return;
        }

        Assert.Null(priced.Pricing);
        Assert.Equal((16505, 0), (priced.PricingRefusal, priced.ActivePricings));
        Assert.Equal(
            authored.PriceOrigin == PriceOrigin.ExternalQuote ? ((string?)V122Catalog.QuoteProvider, 16322) : (null, 0),
            (priced.Provision.QuoteProviderKey, priced.ForeignQuoteAuthority));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_I_the_inventory_authority_and_the_usage_limit_are_authored_without_a_fabricated_quota_and_are_never_a_guarantee(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var stocked = await StockedAsync(variant);
        var limit = Assert.Single(stocked.Policy.PassengerUsageLimits);
        var expected = LimitOf(authored);

        Assert.NotEqual(0, stocked.OfflineRefusal);
        Assert.Equal(("NotConfigured", false), (stocked.Before.State.Name, stocked.Before.IsGuaranteed));
        Assert.Equal(
            (authored.Authority.ToString(), authored.Authority == InventoryAuthority.Unlimited ? "Unlimited" : "DelegatedCheckRequired", false),
            (stocked.Snapshot.Authority!.Name, stocked.Snapshot.State.Name, stocked.Snapshot.IsGuaranteed));
        Assert.Null(stocked.Snapshot.ConfiguredCount);
        Assert.Null(stocked.Snapshot.ConfiguredKg);
        Assert.Null(stocked.Snapshot.Pattern);
        Assert.Equal((await PricedAsync(variant)).Provision.MustCheckAvailability, stocked.Snapshot.RequiresAvailabilityCheck);
        Assert.Equal(
            (expected.LimitScope.ToString(), expected.MaxUnits, authored.Reference, expected.ConsumptionUnit.ToString(), expected.UnitsPerPurchase),
            (limit.LimitScope.Name, limit.MaxUnits, limit.CountingFamilyCode, limit.ConsumptionUnit.Name, limit.UnitsPerPurchase));
        Assert.Equal(("Active", "0|0|0"), (stocked.Policy.Status.Name, stocked.SourceRows));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_Q_the_published_product_reads_back_typed_with_its_selection_contract_and_a_revision_copies_exactly_one_specification(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var read = await ReadAsync(variant);
        var (kind, fields) = V122Catalog.Selection[variant];

        Assert.Equal(("Active", 1), (read.Detail.Status.Name, read.Detail.Version));
        AssertTyped(authored, read.Detail);
        Assert.Equal(
            (kind.ToString(), kind == SelectionKind.QuantityChoice),
            (read.Detail.SelectionContract!.Kind.Name, read.Detail.SelectionContract.ZeroQuantityMeansNoSelection));
        Assert.Equal(fields, read.Detail.SelectionContract.Fields.Select(field => field.Name));
        Assert.Equal((authored.Profile.ToString(), variant, "Active"), (read.Listed.Profile!.Name, read.Listed.VariantCode, read.Listed.Status.Name));
        Assert.Equal(16203, read.ActiveEdit);
        Assert.Equal(("Draft", 2), (read.Revision.Status.Name, read.Revision.Version));
        AssertTyped(authored, read.Revision);
        Assert.Equal(16217, read.VariantDrift);
        Assert.Equal(new[] { $"{authored.Profile}=2" }, read.SpecificationRows);
        Assert.Equal($"{(int)authored.Profile}|{variant}|{(int)authored.Routing}|{(int)ServiceDefinitionStatus.Active}", read.ReadModelRow);
        Assert.Equal(
            ("Active", authored.PriceOrigin.ToString(), V122Catalog.PointOfSale),
            (read.Provision.Status.Name, read.Provision.PriceOrigin.Name, read.Provision.SalesRestrictions!.AllowedPointsOfSale.Single().Value));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Axx_N_the_variant_boundary_is_refused_with_its_business_code(string variant)
    {
        var authored = V122Catalog.Case(variant);
        var negative = Negatives[variant];
        var (airlineId, supplierId) = await OperatorAsync(authored);
        var command = V122Catalog.Define(authored, airlineId, supplierId);

        command = negative.Definition?.Invoke(command) ?? command;

        if (negative.Rule is null && !negative.AtActivation)
        {
            Assert.Equal(negative.Code, await RefusalAsync(scope => scope.DefineServiceDefinition.DefineAsync(command)));
            Assert.False(await RequestAsync(scope => scope.Command.AncillaryServiceDefinitions.AnyAsync(definition => definition.OwnerAirlineId == airlineId)));

            return;
        }

        var definitionId = (await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command))).Id;

        if (negative.Rule is null)
        {
            Assert.Equal(negative.Code, await RefusalAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId))));
            Assert.Equal("Draft", (await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(definitionId))).Status.Name);

            return;
        }

        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId)));

        var rule = negative.Rule(V122Catalog.Rule(authored, definitionId));

        if (!negative.AtActivation)
        {
            Assert.Equal(negative.Code, await RefusalAsync(scope => scope.DefineProvision.DefineAsync(rule)));

            return;
        }

        var provisionId = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(rule))).Id;

        Assert.Equal(negative.Code, await RefusalAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId))));
        Assert.Equal("Draft", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId))).Status.Name);
    }
}
