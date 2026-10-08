using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Boundary;

public class V12BoundaryAcceptanceTests
{
    private const string FrozenAggregate = "AncillaryReservationAggregate";
    private const string FrozenSourceHash = "6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486";
    private const string CommandMigration = "20261008103942_V12Phase1NormalizedProvisionAndPricing.cs";
    private const string QueryMigration = "20261008103947_V12Phase1NormalizedProvisionAndPricingQuery.cs";

    private static readonly string Source = Path.Combine(RepositoryFiles.Root, "src");
    private static readonly string Tests = Path.Combine(RepositoryFiles.Root, "tests");
    private static readonly string AncillaryContracts = Path.Combine(RepositoryFiles.Root, "Contracts", "AeroTech.Messages", "Ancillary");

    private static bool InFolder(string path, string folder)
        => path.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    private static List<string> Files(string root) => Directory
        .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
        .Where(path => !InFolder(path, "obj") && !InFolder(path, "bin"))
        .ToList();

    private static string Relative(string path) => Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/');

    private static List<string> Offending(IEnumerable<string> files, IEnumerable<string> terms)
    {
        var offending = new List<string>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            foreach (var term in terms)
                if (text.Contains(term, StringComparison.OrdinalIgnoreCase) || Path.GetFileName(file).Contains(term, StringComparison.OrdinalIgnoreCase))
                    offending.Add($"{Relative(file)}: {term}");
        }

        return offending;
    }

    private static string UpBody(string project, string migration)
    {
        var text = File.ReadAllText(Path.Combine(Source, project, "Migrations", migration));
        var up = text.IndexOf("void Up(", StringComparison.Ordinal);
        var down = text.IndexOf("void Down(", StringComparison.Ordinal);

        Assert.True(up > 0 && down > up);

        return text[up..down];
    }

    [Fact]
    public void V12_C04_N05_no_evaluator_matcher_simulate_preview_bulk_or_generic_condition_source_exists()
    {
        string[] terms =
        [
            "AncillaryEvaluation", "Evaluator", "Matcher", "EvaluationContext", "FlightContext", "FareContext",
            "EvaluateAncillar", "RuleEngine", "Simulat", "Preview", "Bulk", "ConditionType", "ExpressionTree", "JsonDocument",
            "JsonSerializer", "HasConversion"
        ];

        var files = Files(Source).Concat(Files(AncillaryContracts)).ToList();
        var authored = files.Where(path => !InFolder(path, "Migrations") && !InFolder(path, "AeroTech.Ancillary.ReferenceData")).ToList();

        Assert.True(files.Count > 400);
        Assert.Empty(Offending(files, terms.Take(12)));
        Assert.Empty(Offending(
            authored.Where(path => InFolder(path, "AncillaryProvisionAggregate") || InFolder(path, "AncillaryPricingAggregate")),
            terms.Skip(12)));
        Assert.DoesNotContain(
            Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories),
            directory => terms.Any(term => Path.GetFileName(directory).Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void V12_C04_no_stock_pool_quota_exchange_rate_document_issuing_or_supplier_adapter_is_added()
    {
        var files = Files(Source)
            .Where(path => !InFolder(path, FrozenAggregate) && !InFolder(path, "Migrations"))
            .Concat(Files(AncillaryContracts))
            .ToList();

        Assert.Empty(Offending(files, ["StockPool", "Quota", "Adapter", "ExchangeRate", "CurrencyConversion", "IssueEmd", "EmdIssu", "Inventory"]));
        Assert.Equal(
            new[] { "DependencyInjection.cs" },
            Files(Path.Combine(Source, "AeroTech.Ancillary.Providers")).Select(Path.GetFileName));
    }

    [Fact]
    public void V12_C04_the_reservation_use_cases_are_still_exactly_hold_get_and_confirm_and_their_source_is_byte_stable()
    {
        static string[] Folders(params string[] segments) => Directory
            .EnumerateDirectories(Path.Combine([Source, .. segments]))
            .Select(directory => Path.GetFileName(directory)!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "ConfirmAncillaryHold", "HoldAncillaryServices" },
            Folders("AeroTech.Ancillary.Application", FrozenAggregate, "Commands"));
        Assert.Equal(
            new[] { "GetAncillaryHoldById" },
            Folders("AeroTech.Ancillary.Query", FrozenAggregate, "Queries"));
        Assert.DoesNotContain(
            Files(Source).Select(path => Path.GetFileName(path)!),
            name => new[] { "Cancel", "Release", "Issue", "Split", "History" }.Any(term => name.Contains(term, StringComparison.Ordinal)));

        var frozen = Files(Source)
            .Concat(Files(Tests))
            .Where(path => InFolder(path, FrozenAggregate)
                           || Path.GetFileName(path) is "M1ReservationAcceptanceTests.cs" or "M1ReservationConformanceTests.cs")
            .Select(path => (Path: Relative(path), Hash: Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path).Where(value => value != 13).ToArray()))))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(36, frozen.Count);
        Assert.Equal(
            FrozenSourceHash,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', frozen.Select(file => $"{file.Path}:{file.Hash}"))))));
    }

    [Fact]
    public void V12_C04_no_integration_contract_for_another_module_is_added()
    {
        var contracts = Files(AncillaryContracts);

        Assert.NotEmpty(contracts);
        Assert.All(contracts, path => Assert.Equal("Enums", Path.GetFileName(Path.GetDirectoryName(path))));
        Assert.Contains(contracts, path => Path.GetFileName(path) == "PricingUnit.cs");
        Assert.Contains(contracts, path => Path.GetFileName(path) == "PricingStatus.cs");
        Assert.Contains(contracts, path => Path.GetFileName(path) == "DayTimeRestrictionEffect.cs");

        var namespaces = Files(Source)
            .SelectMany(File.ReadAllLines)
            .Select(line => Regex.Match(line, @"^\s*using (?:[A-Za-z]+ = )?(AeroTech\.Messages\.[A-Za-z]+)"))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "AeroTech.Messages.AirPrice", "AeroTech.Messages.Ancillary", "AeroTech.Messages.Core", "AeroTech.Messages.Shared" },
            namespaces);
        Assert.Empty(Offending(
            Files(Source).Where(path => !InFolder(path, "AeroTech.Ancillary.ReferenceData")),
            ["HttpClient", "AirAvail", "FlightFlow", "JetPay"]));
    }

    [Fact]
    public void V12_C02_C04_the_rest_api_exposes_exactly_the_authoring_routes_and_the_frozen_service_routes()
    {
        var routes = new List<string>();

        foreach (var file in Files(Path.Combine(Source, "AeroTech.Ancillary.RestApi")))
        {
            var text = File.ReadAllText(file);
            var route = Regex.Match(text, @"\[Route\(\$?""([^""]+)""\)\]");

            if (!route.Success)
                continue;

            var prefix = Regex.Replace(route.Groups[1].Value, @"v\{+version:apiVersion\}+/", string.Empty);

            foreach (Match action in Regex.Matches(text, @"\[Http(Get|Post|Put|Patch|Delete)(?:\(""([^""]*)""\))?\]"))
                routes.Add($"{action.Groups[1].Value.ToUpperInvariant()} {prefix}{(action.Groups[2].Success ? "/" + action.Groups[2].Value : string.Empty)}");
        }

        string[] preserved =
        [
            "GET api/[controller]",
            "POST Backoffice/Suppliers",
            "GET Backoffice/Suppliers/Paginated",
            "GET Backoffice/Suppliers/{supplierId:long}",
            "POST Backoffice/Suppliers/{supplierId:long}/Retire",
            "POST Backoffice/AncillaryServiceDefinitions",
            "GET Backoffice/AncillaryServiceDefinitions/Paginated",
            "GET Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}",
            "PUT Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}",
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/Activate",
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/Suspend",
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/Reactivate",
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/Retire",
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/Revise",
            "POST Backoffice/AncillaryProvisions",
            "GET Backoffice/AncillaryProvisions/Paginated",
            "GET Backoffice/AncillaryProvisions/{provisionId:long}",
            "PUT Backoffice/AncillaryProvisions/{provisionId:long}",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/Activate",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/Suspend",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/Reactivate",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/Retire",
            "POST Service/Ancillaries/Service-Holds",
            "GET Service/Ancillaries/Service-Holds/{holdId:long}",
            "POST Service/Ancillaries/Service-Holds/{holdId:long}/Confirmations"
        ];

        string[] added =
        [
            "POST Backoffice/AncillaryServiceDefinitions/{serviceDefinitionId:long}/AssignPricingUnit",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/Publish",
            "POST Backoffice/AncillaryProvisions/{provisionId:long}/SwitchActivePricing",
            "POST Backoffice/AncillaryPricings",
            "GET Backoffice/AncillaryPricings/Paginated",
            "GET Backoffice/AncillaryPricings/{pricingId:long}",
            "PUT Backoffice/AncillaryPricings/{pricingId:long}",
            "POST Backoffice/AncillaryPricings/{pricingId:long}/Activate",
            "POST Backoffice/AncillaryPricings/{pricingId:long}/Suspend",
            "POST Backoffice/AncillaryPricings/{pricingId:long}/Reactivate",
            "POST Backoffice/AncillaryPricings/{pricingId:long}/Retire",
            "POST Backoffice/AncillaryPricings/{pricingId:long}/Revise"
        ];

        var rows = new[] { "TravelDates", "SeasonalPeriods", "BlackoutPeriods", "DayTimeRestrictions" }
            .SelectMany(collection => new[]
            {
                $"POST Backoffice/AncillaryProvisions/{{provisionId:long}}/{collection}",
                $"PUT Backoffice/AncillaryProvisions/{{provisionId:long}}/{collection}/{{rowId:long}}",
                $"DELETE Backoffice/AncillaryProvisions/{{provisionId:long}}/{collection}/{{rowId:long}}"
            });

        Assert.Equal(
            preserved.Concat(added).Concat(rows).OrderBy(route => route, StringComparer.Ordinal),
            routes.OrderBy(route => route, StringComparer.Ordinal));
        Assert.Equal(49, routes.Count);
        Assert.All(preserved, route => Assert.Contains(route, routes));
    }

    [Fact]
    public void V12_C03_the_v12_migrations_only_add_objects_and_copy_rows_and_never_drop_or_rewrite_v11_data()
    {
        (string Project, string Migration, string[] Operations)[] migrations =
        [
            ("AeroTech.Ancillary.Persistence", CommandMigration, ["AddColumn", "CreateIndex", "CreateTable", "DropIndex", "Sql"]),
            ("AeroTech.Ancillary.Query", QueryMigration, ["AddColumn", "CreateIndex", "CreateTable", "Sql"])
        ];

        foreach (var (project, migration, operations) in migrations)
        {
            var up = UpBody(project, migration);

            Assert.Equal(
                operations,
                Regex.Matches(up, @"migrationBuilder\.([A-Za-z]+)").Select(match => match.Groups[1].Value).Distinct().OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal(26, Regex.Matches(up, @"CreateTable\(\s*name: ""([A-Za-z]+)""").Count);
            Assert.Equal(24, Regex.Matches(up, @"migrationBuilder\.Sql\(").Count);
            Assert.Equal(
                new[] { "INSERT INTO" },
                Regex.Matches(up, @"\b(INSERT INTO|UPDATE|DELETE|DROP|TRUNCATE|ALTER|MERGE)\b").Select(match => match.Value).Distinct());
            Assert.Equal(
                "PricingUnit",
                Regex.Matches(up, @"AddColumn<[a-z?]+>\(\s*name: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).Single());
            Assert.Contains("nullable: true", Regex.Match(up, @"AddColumn<[^;]+;").Value, StringComparison.Ordinal);
            Assert.DoesNotContain("Reservation", up, StringComparison.Ordinal);
            Assert.DoesNotContain("Supplier", up.Replace("SupplierId", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        }

        Assert.Equal(
            "IX_ProvisionRoutePairs_AncillaryProvisionId",
            Regex.Match(UpBody("AeroTech.Ancillary.Persistence", CommandMigration), @"DropIndex\(\s*name: ""([A-Za-z_]+)""").Groups[1].Value);
    }

    [Fact]
    public void V12_C02_every_command_service_of_the_application_is_registered_once_as_scoped()
    {
        var services = new ServiceCollection().AddApplication(new ConfigurationBuilder().Build());

        var contracts = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type.IsInterface
                           && type.Namespace is not null
                           && type.Namespace.Contains(".Commands.", StringComparison.Ordinal)
                           && type.Name.EndsWith("Service", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(39, contracts.Count);
        Assert.Equal(8, contracts.Count(contract => contract.Namespace!.Contains(".AncillaryPricingAggregate.", StringComparison.Ordinal)));
        Assert.Equal(2, contracts.Count(contract => contract.Namespace!.Contains($".{FrozenAggregate}.", StringComparison.Ordinal)));

        foreach (var contract in contracts)
        {
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);

            Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
            Assert.Equal(contract.Name[1..], registration.ImplementationType!.Name);
        }
    }
}
