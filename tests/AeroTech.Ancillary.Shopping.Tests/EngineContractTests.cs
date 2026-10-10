using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EngineContractTests
{
    private static AncillaryShoppingContext RoundTrip(params ShoppingTraveller[] travellers)
    {
        var outbound = Trip.Flight("F1", 101, Trip.Thr, Trip.Ist);
        var inbound = Trip.Flight("F2", 102, Trip.Ist, Trip.Ika, outbound.DepartureAt.AddDays(7), Trip.Istanbul);

        return Trip.Context([outbound, inbound], [Trip.Portion("P1", 1, outbound), Trip.Portion("P2", 2, inbound)], travellers.Length == 0 ? null : travellers);
    }

    [Fact]
    public async Task ENGINE_01_a_two_flight_portion_yields_one_portion_priced_candidate_and_sector_candidates_only_when_authored()
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");
        var wifi = lab.Definition("A24");

        lab.Price(bag, lab.Provision(bag, scope: ServiceCoverageScope.Portion), ShoppingLab.Rate(40m));
        lab.Price(wifi, lab.Provision(wifi, scope: ServiceCoverageScope.Sector), ShoppingLab.Rate(12m));

        var first = Trip.Flight("F1", 101, Trip.Thr, Trip.Ist);
        var second = Trip.Flight("F2", 102, Trip.Ist, Trip.Mhd, first.DepartureAt.AddHours(6), Trip.Istanbul);
        var result = await lab.Engine.ShopAsync(Trip.Context([first, second]));
        var portion = result.One(bag.ServiceDefinitionRef);
        var sectors = result.Candidates.Where(candidate => candidate.ServiceDefinitionRef == wifi.ServiceDefinitionRef).ToList();

        Assert.Equal((ServiceCoverageScope.Portion, 40m), (portion.Scope.CoverageScope, portion.Price.CompleteUnitTotal));
        Assert.Equal(new[] { "F1", "F2" }, portion.FlightRefs);
        Assert.Equal(new[] { "P1" }, portion.PortionRefs);
        Assert.Equal(new[] { "F1", "F2" }, sectors.Select(candidate => Assert.Single(candidate.FlightRefs)));
        Assert.All(sectors, candidate => Assert.Equal(12m, candidate.Price.CompleteUnitTotal));
    }

    [Fact]
    public async Task ENGINE_02_the_same_context_gives_the_same_ordered_content_whatever_the_source_provenance()
    {
        var lab = new ShoppingLab();

        foreach (var code in new[] { "A24", "A01", "A12", "A23", "A15" })
            lab.Sellable(code);

        var context = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Adult("T2")]);
        var first = await lab.Engine.ShopAsync(context);
        var second = await lab.Engine.ShopAsync(context);
        var direct = await lab.Engine.ShopAsync(context with
        {
            SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Direct, TrustedSourceReference = "DIRECT-9", TrustedSourceVersion = "v1" }
        });

        static IEnumerable<string> Shape(CanonicalAncillaryOfferResult result)
            => result.Candidates.Select(candidate => $"{candidate.ServiceDefinitionRef}|{candidate.CandidateIdentity}|{candidate.OfferReadiness}|{candidate.Price.CompleteUnitTotal}|{string.Join(',', candidate.ReasonCodes)}");

        Assert.Equal(10, first.Candidates.Count);
        Assert.Equal(Shape(first), Shape(second));
        Assert.Equal(Shape(first), Shape(direct));
        Assert.Equal(
            first.Candidates.Select(candidate => candidate.ServiceDefinitionRef).Distinct().Order(StringComparer.Ordinal),
            first.Candidates.Select(candidate => candidate.ServiceDefinitionRef).Distinct());
    }

    [Fact]
    public async Task ENGINE_03_only_the_active_version_its_active_provision_and_the_active_pricing_of_that_provision_are_used()
    {
        var lab = new ShoppingLab();
        var meal = lab.Definition("A12", "MEAL_LIVE");
        var provision = lab.Provision(meal);
        var wifi = lab.Definition("A24", "WIFI_NO_PRICE");

        lab.Definition("A12", "MEAL_DRAFT", activate: false);
        lab.Price(meal, provision, ShoppingLab.Rate(15m));
        lab.Provision(wifi);
        lab.Catalog.Unfiltered = true;

        var live = await lab.Engine.ShopAsync(Trip.Context());

        Assert.Equal((meal.ServiceDefinitionRef, 15m), (live.One().ServiceDefinitionRef, live.One().Price.CompleteUnitTotal));

        var unpriced = (await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true })).One(wifi.ServiceDefinitionRef);

        Assert.Equal((OfferReadiness.Unavailable, PriceAssessmentStatus.Unavailable), (unpriced.OfferReadiness, unpriced.Price.Status));
        Assert.Contains(ShoppingReasonCodes.PricingNotActive, unpriced.ReasonCodes);
        Assert.Null(unpriced.Price.CompleteUnitTotal);

        provision.Suspend(ShoppingLab.Now);

        var suspended = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });

        Assert.DoesNotContain(suspended.Candidates, candidate => candidate.ServiceDefinitionRef is "MEAL_LIVE" or "MEAL_DRAFT");
    }

    [Fact]
    public async Task ENGINE_04_another_point_of_sale_never_sees_the_terms_and_a_filter_cannot_widen_the_scope()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi, sequence: 10, pos: ShoppingLab.Pos), ShoppingLab.Rate(10m));
        lab.Price(wifi, lab.Provision(wifi, sequence: 20, pos: ShoppingLab.OtherPos), ShoppingLab.Rate(25m));
        lab.Catalog.Unfiltered = true;

        var own = await lab.Engine.ShopAsync(Trip.Context());
        var other = await lab.Engine.ShopAsync(Trip.Context(pointOfSaleId: ShoppingLab.OtherPos));
        var foreign = await lab.Engine.ShopAsync(
            Trip.Context(pointOfSaleId: 9003),
            new ShoppingFilter { ServiceDefinitionRefs = [wifi.ServiceDefinitionRef], VariantCodes = ["A24"], IncludeNonSellable = true });

        Assert.Equal((10m, 10), (own.One().Price.CompleteUnitTotal, own.One().ProvisionSequence));
        Assert.Equal((25m, 20), (other.One().Price.CompleteUnitTotal, other.One().ProvisionSequence));
        Assert.Empty(foreign.Candidates);
        Assert.Empty(foreign.Diagnostics);
    }

    [Fact]
    public async Task ENGINE_05_every_variant_returns_the_selection_contract_of_the_existing_factory()
    {
        var lab = new ShoppingLab();

        foreach (var variant in V122Catalog.Cases)
            lab.Sellable(variant.Code);

        var result = await lab.Engine.ShopAsync(RoundTrip(Trip.Adult("T1"), Trip.Child("T2")));

        Assert.Equal(AncillaryVariant.All.Select(variant => variant.Code).Order(), result.Candidates.Select(candidate => candidate.VariantCode).Distinct().Order());

        foreach (var candidate in result.Candidates)
        {
            var definition = lab.Catalog.Definitions.Single(row => row.Id == candidate.DefinitionVersionId);
            var expected = V122Catalog.Selection[candidate.VariantCode];

            Assert.Equal(definition.Profile, candidate.Profile);
            Assert.Equal(definition.SelectionContract!.Kind, candidate.Selection.Kind);
            Assert.Equal(definition.SelectionContract.Fields, candidate.Selection.Fields);
            Assert.Equal(expected.Kind, candidate.Selection.Kind);
            Assert.Equal(expected.Fields, candidate.Selection.Fields.Select(field => field.Name));
        }
    }

    [Fact]
    public async Task ENGINE_06_incomplete_age_fare_baggage_or_time_zone_facts_never_become_eligible()
    {
        var lab = new ShoppingLab();
        var (meal, _) = lab.Sellable("A12", rules: ShoppingLab.Rules(passengers: new([], [new(2, 12)])));
        var (wifi, _) = lab.Sellable("A24", rules: ShoppingLab.Rules(fares: new([], [], [5], [], [], [])));
        var (bag, _) = lab.Sellable("A01");
        var (priority, _) = lab.Sellable(
            "A23",
            rules: ShoppingLab.Rules(dayTime: new([new(127, new TimeOnly(6, 0), new TimeOnly(22, 0), DayTimeRestrictionEffect.Allow)])));
        var context = Trip.Context([Trip.Flight(zone: null)], travellers: [Trip.Child("T1") with { DateOfBirth = null }]) with
        {
            TravellerFareFacts = [],
            TravellerBaggageFacts = []
        };
        var result = await lab.Engine.ShopAsync(context);

        Assert.Equal(4, result.Candidates.Count);
        Assert.All(result.Candidates, candidate =>
        {
            Assert.Equal(EligibilityStatus.InsufficientContext, candidate.Eligibility.Status);
            Assert.Equal(OfferReadiness.NeedsVerification, candidate.OfferReadiness);
            Assert.NotEmpty(candidate.Eligibility.MissingContextFields);
        });
        Assert.True(result.HasBlockedCandidates);
        Assert.Contains(ShoppingReasonCodes.TimeZoneNotVerified, result.One(meal.ServiceDefinitionRef).ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.FareFactsNotVerified, result.One(wifi.ServiceDefinitionRef).ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.BaggageAllowanceNotVerified, result.One(bag.ServiceDefinitionRef).ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.TimeZoneNotVerified, result.One(priority.ServiceDefinitionRef).ReasonCodes);
    }

    [Fact]
    public async Task ENGINE_07_a_candidate_identity_is_only_an_internal_address_and_is_re_evaluated_against_the_context()
    {
        var lab = new ShoppingLab();
        var (wifi, provision) = lab.Sellable("A24");
        var candidate = (await lab.Engine.ShopAsync(Trip.Context())).One();
        var identity = candidate.CandidateIdentity;

        Assert.Equal(new CandidateIdentity(wifi.Id, provision.Id, provision.CoverageScope, "T1", "F1", candidate.Price.PricingRevisionId), identity);
        Assert.DoesNotContain(
            typeof(CandidateIdentity).GetProperties(),
            property => property.Name.Contains("Offer", StringComparison.Ordinal) || property.Name.Contains("Token", StringComparison.Ordinal));

        foreach (var forged in new[]
                 {
                     identity with { DefinitionVersionId = 1 },
                     identity with { ProvisionId = 1 },
                     identity with { TravellerRef = "T9" },
                     identity with { CoverageRef = "F9" },
                     identity with { CoverageScope = ServiceCoverageScope.Journey }
                 })
        {
            var refused = await Assert.ThrowsAsync<BusinessException>(() => lab.Engine.EvaluateSelectionAsync(Trip.Context(), forged, new AncillarySelection { PlanCode = "FULL" }));

            Assert.Equal((16802, 404), (refused.Code, refused.HttpStatus));
        }

        var elsewhere = await Assert.ThrowsAsync<BusinessException>(
            () => lab.Engine.EvaluateSelectionAsync(Trip.Context(pointOfSaleId: ShoppingLab.OtherPos), identity, new AncillarySelection { PlanCode = "FULL" }));

        Assert.Equal(16802, elsewhere.Code);
    }

    [Fact]
    public async Task ENGINE_08_the_engine_only_reads_in_a_fixed_number_of_batches_and_the_module_exposes_no_route_hold_or_write()
    {
        var lab = new ShoppingLab();

        foreach (var variant in V122Catalog.Cases)
            lab.Sellable(variant.Code);

        var before = lab.Catalog.Provisions.Select(provision => (provision.Id, provision.Status, provision.Sequence)).ToList();
        var reads = lab.Catalog.Reads;

        await lab.Engine.ShopAsync(RoundTrip(Trip.Adult("T1"), Trip.Adult("T2"), Trip.Child("T3")));

        Assert.Equal(5, lab.Catalog.Reads - reads);
        Assert.Equal(before, lab.Catalog.Provisions.Select(provision => (provision.Id, provision.Status, provision.Sequence)));

        var source = Directory
            .EnumerateFiles(Path.Combine(RepositoryFiles.Root, "src", "AeroTech.Ancillary.Shopping"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Select(path => (Path: path, Text: File.ReadAllText(path)))
            .ToList();

        Assert.True(source.Count > 40);

        foreach (var term in new[]
                 {
                     "[Route", "[Http", "ControllerBase", "IRequest<", "SaveChanges", "AncillaryReservation", "HoldAncillary", "ConfirmAncillary", "IUnitOfWork", "HttpClient",
                     "AirAvail", "AeroTech.Ordering", "Messages.Ordering", "Dictionary<string, object", ".AddAsync(", ".Update(", ".Remove(", "ExecuteSql", "ExecuteUpdate",
                     "ExecuteDelete"
                 })
            Assert.DoesNotContain(source, file => file.Text.Contains(term, StringComparison.Ordinal));

        Assert.All(
            source.Where(file => file.Path.Contains($"{Path.DirectorySeparatorChar}Sql{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && file.Text.Contains("_dbContext.", StringComparison.Ordinal)),
            file => Assert.Equal(file.Text.Split("_dbContext.Ancillary").Length + file.Text.Split("_dbContext.Flight").Length + file.Text.Split("_dbContext.Airport").Length - 3, file.Text.Split(".AsNoTracking()").Length - 1));
    }
}
