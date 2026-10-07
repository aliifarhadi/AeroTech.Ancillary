using System.Text.RegularExpressions;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Boundary;

public class P1BoundaryAcceptanceTests
{
    private const string FrozenAggregate = "AncillaryReservationAggregate";
    private const string CommandMigration = "20261007202914_V11Phase1CommercialAuthoring.cs";
    private const string QueryMigration = "20261007202918_V11Phase1CommercialAuthoringQuery.cs";

    private static readonly string Source = Path.Combine(RepositoryFiles.Root, "src");
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
    public void P1_A02_A05_K07_no_evaluator_matcher_simulate_preview_or_bulk_source_exists()
    {
        string[] terms =
        [
            "AncillaryEvaluation", "Evaluator", "Matcher", "EvaluationContext", "FlightContext", "FareContext",
            "EvaluateAncillar", "RuleEngine", "Simulat", "Preview", "Bulk"
        ];

        var files = Files(Source).Concat(Files(AncillaryContracts)).ToList();

        Assert.True(files.Count > 200);
        Assert.Empty(Offending(files, terms));
        Assert.DoesNotContain(
            Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories),
            directory => terms.Any(term => Path.GetFileName(directory).Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void P1_REQ_no_stock_pool_quota_or_supplier_adapter_is_added_outside_the_frozen_baseline()
    {
        var files = Files(Source)
            .Where(path => !InFolder(path, FrozenAggregate) && !InFolder(path, "Migrations"))
            .Concat(Files(AncillaryContracts))
            .ToList();

        Assert.Empty(Offending(files, ["StockPool", "Quota", "Adapter"]));
        Assert.Equal(
            new[] { "DependencyInjection.cs" },
            Files(Path.Combine(Source, "AeroTech.Ancillary.Providers")).Select(Path.GetFileName));
    }

    [Fact]
    public void P1_A03_the_reservation_use_cases_are_still_exactly_hold_get_and_confirm()
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
    }

    [Fact]
    public void P1_A04_no_integration_contract_for_another_module_is_added()
    {
        var contracts = Files(AncillaryContracts);

        Assert.NotEmpty(contracts);
        Assert.All(contracts, path => Assert.Equal("Enums", Path.GetFileName(Path.GetDirectoryName(path))));

        var namespaces = Files(Source)
            .SelectMany(File.ReadAllLines)
            .Select(line => Regex.Match(line, @"^\s*using (AeroTech\.Messages\.[A-Za-z]+)"))
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
            ["HttpClient", "AirAvail", "FlightFlow"]));
    }

    [Fact]
    public void P1_A05_K07_the_rest_api_exposes_exactly_the_authoring_routes_and_the_frozen_service_routes()
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

        string[] expected =
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

        Assert.Equal(expected.OrderBy(route => route, StringComparer.Ordinal), routes.OrderBy(route => route, StringComparer.Ordinal));
    }

    [Fact]
    public void P1_REQ_the_v11_migrations_only_add_authoring_columns_and_the_route_pair_tables()
    {
        (string Project, string Migration, string Table, string[] Touched)[] migrations =
        [
            ("AeroTech.Ancillary.Persistence", CommandMigration, "ProvisionRoutePairs",
                ["AncillaryProvisions", "ProvisionPriceLines", "ProvisionRoutePairs"]),
            ("AeroTech.Ancillary.Query", QueryMigration, "AncillaryProvisionRoutePairs",
                ["AncillaryProvisionPriceLines", "AncillaryProvisionRoutePairs", "AncillaryProvisions", "AncillaryServiceDefinitions"])
        ];

        foreach (var (project, migration, table, touched) in migrations)
        {
            var up = UpBody(project, migration);

            Assert.Equal(
                new[] { "AddColumn", "CreateIndex", "CreateTable" },
                Regex.Matches(up, @"migrationBuilder\.([A-Za-z]+)").Select(match => match.Groups[1].Value).Distinct().OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal(
                table,
                Regex.Matches(up, @"CreateTable\(\s*name: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).Single());
            Assert.Equal(
                touched,
                Regex.Matches(up, @"table: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).Distinct().OrderBy(name => name, StringComparer.Ordinal));
            Assert.DoesNotContain("Reservation", up, StringComparison.Ordinal);
            Assert.DoesNotContain("Supplier", up.Replace("SupplierId", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void P1_REQ_every_command_service_of_the_application_is_registered_once_as_scoped()
    {
        var services = new ServiceCollection().AddApplication(new ConfigurationBuilder().Build());

        var contracts = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type.IsInterface
                           && type.Namespace is not null
                           && type.Namespace.Contains(".Commands.", StringComparison.Ordinal)
                           && type.Name.EndsWith("Service", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(17, contracts.Count);

        foreach (var contract in contracts)
        {
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);

            Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
            Assert.Equal(contract.Name[1..], registration.ImplementationType!.Name);
        }
    }
}
