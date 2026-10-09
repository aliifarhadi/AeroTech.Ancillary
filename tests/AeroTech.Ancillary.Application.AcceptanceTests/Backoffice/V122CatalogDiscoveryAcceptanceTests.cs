using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class V122CatalogDiscoveryAcceptanceTests
{
    private static readonly string[] Revised = ["A01", "A14", "A20", "A24"];

    private static readonly string[] Canonical =
    [
        "A01|Extra checked bag|Baggage|Baggage|PerPiece|FlightDeparture",
        "A02|Extra weight package|Baggage|Baggage|PerItem|FlightDeparture",
        "A03|Overweight bag|Baggage|Baggage|PerPiece,PerKilogram|FlightDeparture",
        "A04|Oversize bag|Baggage|Baggage|PerPiece|FlightDeparture",
        "A05|Cabin bag|Baggage|Baggage|PerPiece|FlightDeparture",
        "A06|Sports and special equipment|Baggage|Baggage|PerItem,PerPiece|FlightDeparture",
        "A07|Standard seat|Seat|Seat|PerSeat|FlightDeparture",
        "A08|Preferred seat|Seat|Seat|PerSeat|FlightDeparture",
        "A09|Extra seat|Seat|Seat|PerSeat|FlightDeparture",
        "A10|Cabin upgrade|Upgrade|Standard|PerPassenger,PerSeat|FlightDeparture",
        "A11|Free special meal|Meal|Standard|PerPassenger,PerItem|FlightDeparture",
        "A12|Paid pre-order meal|Meal|Standard|PerItem,PerPassenger|FlightDeparture",
        "A13|Pet in cabin|Pet|Standard|PerItem|FlightDeparture",
        "A14|Pet in hold|Pet|Standard|PerItem|FlightDeparture",
        "A15|Wheelchair|AssistedTravel|Standard|PerPassenger|FlightDeparture",
        "A16|Disability assistance|AssistedTravel|Standard|PerPassenger|FlightDeparture",
        "A17|Medical equipment|AssistedTravel|Standard|PerPassenger,PerItem|FlightDeparture",
        "A18|Bassinet|AssistedTravel|Standard|PerPassenger,PerItem|FlightDeparture",
        "A19|Unaccompanied minor|AssistedTravel|Standard|PerPassenger|FlightDeparture",
        "A20|Lounge|AirportService|Standard|PerPassenger|ServiceStart,FlightDeparture",
        "A21|Fast track|AirportService|Standard|PerPassenger|ServiceStart,FlightDeparture",
        "A22|CIP meet and assist|AirportService|Standard|PerPassenger,PerItem|ServiceStart,FlightDeparture",
        "A23|Priority boarding and check-in|Priority|Standard|PerPassenger|FlightDeparture",
        "A24|Onboard Wi-Fi|Connectivity|Standard|PerItem,PerPassenger|FlightDeparture"
    ];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V122CatalogDiscoveryAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private sealed record Listed(long Id, string Reference, int Version, AncillaryProfile Profile, string VariantCode);

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(Action<BackofficeGetAncillaryServiceDefinitionsPaginatedQuery> filter)
        => BusinessAssert.ThrowsAsync(16202, 422, () => RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(Query(filter, 1, 10))));

    private static BackofficeGetAncillaryServiceDefinitionsPaginatedQuery Query(
        Action<BackofficeGetAncillaryServiceDefinitionsPaginatedQuery> filter,
        int pageNumber,
        int pageSize)
    {
        var query = new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { PageNumber = pageNumber, PageSize = pageSize };

        filter(query);

        return query;
    }

    private async Task<List<ServiceDefinitionPaginatedRowDto>> PagedAsync(
        Action<BackofficeGetAncillaryServiceDefinitionsPaginatedQuery> filter,
        int pageSize,
        IReadOnlyCollection<Listed> matching)
    {
        var expected = matching
            .OrderBy(row => row.Reference, StringComparer.Ordinal)
            .ThenByDescending(row => row.Version)
            .ThenBy(row => row.Id)
            .ToList();
        var rows = new List<ServiceDefinitionPaginatedRowDto>();
        var pages = Math.Max(1, (int)Math.Ceiling(expected.Count / (double)pageSize));

        for (var pageNumber = 1; pageNumber <= pages + 1; pageNumber++)
        {
            var commands = _database.Sql.Count;
            var page = await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(Query(filter, pageNumber, pageSize)));

            Assert.Equal(2, _database.Sql.Count - commands);
            Assert.Equal(expected.Count, page.TotalCount);
            Assert.Equal(pageNumber, page.PageNumber);
            Assert.Equal(
                expected.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(row => row.Id.ToString()),
                page.Results.Select(row => row.Id));
            rows.AddRange(page.Results);
        }

        Assert.Equal(
            expected.Select(row => (row.Reference, row.Version, row.Profile.ToString(), row.VariantCode)),
            rows.Select(row => (row.ServiceDefinitionRef, row.Version, row.Profile!.Name, row.VariantCode!)));

        return rows;
    }

    private async Task<List<Listed>> MixedCatalogAsync(int airlineId)
    {
        var local = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var external = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider));
        var catalog = new List<Listed>();

        for (var index = 0; index < V122Catalog.Cases.Count; index++)
        {
            var variant = V122Catalog.Cases[index];
            var reference = $"L{index * 7 % 24:00}_{variant.Code}";
            var versioned = Revised.Contains(variant.Code);
            var definition = await _proof.DefinitionAsync(
                V122Catalog.Define(variant, airlineId, V122Catalog.NeedsExternalSupplier(variant) ? external : local) with { ServiceDefinitionRef = reference },
                versioned);

            catalog.Add(new Listed(definition.Id, reference, 1, variant.Profile, variant.Code));

            if (!versioned)
                continue;

            var revision = await RequestAsync(scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(definition.Id)));

            catalog.Add(new Listed(revision.Id, reference, 2, variant.Profile, variant.Code));
        }

        return catalog;
    }

    [Fact]
    public async Task V122_L01_the_definition_list_filters_by_profile_and_variant_before_it_counts_and_pages()
    {
        var airlineId = _database.NextAirlineId();
        var catalog = await MixedCatalogAsync(airlineId);

        Assert.Equal(28, catalog.Count);

        var everything = await PagedAsync(query => query.OwnerAirlineId = airlineId, 10, catalog);

        Assert.True(everything.Take(10).Select(row => row.Profile!.Name).Distinct().Count() > 4);
        Assert.Equal(Enum.GetNames<AncillaryProfile>().Order(), everything.Select(row => row.Profile!.Name).Distinct().Order());

        foreach (var profile in Enum.GetValues<AncillaryProfile>())
        {
            var matching = catalog.Where(row => row.Profile == profile).ToList();
            var rows = await PagedAsync(
                query =>
                {
                    query.OwnerAirlineId = airlineId;
                    query.Profile = profile;
                },
                2,
                matching);

            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Equal((int)profile, row.Profile!.Value));
        }

        foreach (var variant in V122Catalog.Cases)
        {
            var matching = catalog.Where(row => row.VariantCode == variant.Code).ToList();

            await PagedAsync(
                query =>
                {
                    query.OwnerAirlineId = airlineId;
                    query.VariantCode = variant.Code;
                },
                1,
                matching);
            await PagedAsync(
                query =>
                {
                    query.OwnerAirlineId = airlineId;
                    query.Profile = variant.Profile;
                    query.VariantCode = variant.Code;
                },
                10,
                matching);
        }
    }

    [Fact]
    public async Task V122_L02_the_profile_and_variant_filters_combine_with_the_existing_filters_and_keep_the_list_order()
    {
        var airlineId = _database.NextAirlineId();
        var catalog = await MixedCatalogAsync(airlineId);
        var pets = catalog.Where(row => row.Profile == AncillaryProfile.Pet).ToList();

        Assert.Equal(new[] { ("A13", 1), ("A14", 1), ("A14", 2) }, pets.Select(row => (row.VariantCode, row.Version)).Order());

        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.Profile = AncillaryProfile.Pet;
                query.Status = ServiceDefinitionStatus.Active;
            },
            10,
            pets.Where(row => row is { VariantCode: "A14", Version: 1 }).ToList());
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.Profile = AncillaryProfile.Pet;
                query.Status = ServiceDefinitionStatus.Draft;
            },
            1,
            pets.Where(row => row is not { VariantCode: "A14", Version: 1 }).ToList());
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.Search = "_A2";
            },
            3,
            catalog.Where(row => row.VariantCode.StartsWith("A2", StringComparison.Ordinal)).ToList());
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.Search = "_A2";
                query.Profile = AncillaryProfile.AirportService;
            },
            3,
            catalog.Where(row => row.Profile == AncillaryProfile.AirportService).ToList());
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.ServiceDefinitionRef = catalog.Single(row => row is { VariantCode: "A20", Version: 2 }).Reference;
                query.VariantCode = "A20";
            },
            1,
            catalog.Where(row => row.VariantCode == "A20").ToList());
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = airlineId;
                query.ServiceDefinitionRef = catalog.Single(row => row.VariantCode == "A21").Reference;
                query.VariantCode = "A20";
            },
            10,
            []);
        await PagedAsync(
            query =>
            {
                query.OwnerAirlineId = _database.NextAirlineId();
                query.Profile = AncillaryProfile.Baggage;
            },
            10,
            []);
    }

    [Fact]
    public async Task V122_L03_an_unknown_variant_or_a_variant_of_another_profile_is_refused_from_the_canonical_registry()
    {
        var airlineId = _database.NextAirlineId();

        await RefusedAsync(query => query.VariantCode = "A99");
        await RefusedAsync(query => query.VariantCode = "a01");
        await RefusedAsync(query => query.VariantCode = " A01");
        await RefusedAsync(query =>
        {
            query.OwnerAirlineId = airlineId;
            query.Profile = AncillaryProfile.Pet;
            query.VariantCode = "A01";
        });

        foreach (var variant in AncillaryVariant.All)
        foreach (var profile in Enum.GetValues<AncillaryProfile>().Where(profile => profile != variant.Profile))
            await RefusedAsync(query =>
            {
                query.Profile = profile;
                query.VariantCode = variant.Code;
            });

        var accepted = await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, Profile = AncillaryProfile.Baggage, VariantCode = "A01" }));

        Assert.Equal(0, accepted.TotalCount);
        Assert.Contains("Profile", accepted.Metadata.Fields.Select(field => field.Title));
        Assert.Contains("Variant", accepted.Metadata.Fields.Select(field => field.Title));
    }

    [Fact]
    public async Task V122_V01_the_variant_discovery_returns_the_twenty_four_canonical_entries_with_their_allowed_units_and_date_bases()
    {
        var variants = await RequestAsync(scope => scope.GetVariants.ExecuteAsync(new BackofficeGetAncillaryVariantsQuery()));

        Assert.Equal(24, variants.Count);
        Assert.Equal(
            Canonical,
            variants.Select(variant => string.Join(
                '|',
                variant.Code,
                variant.Name,
                variant.Profile.Name,
                variant.ApplicationType.Name,
                string.Join(',', variant.PricingUnits.Select(unit => unit.Name)),
                string.Join(',', variant.ServiceDateBases.Select(basis => basis.Name)))));
        Assert.Equal(
            AncillaryVariant.All.OrderBy(variant => variant.Code, StringComparer.Ordinal).Select(variant => (
                variant.Code,
                (int)variant.Profile,
                (int)variant.ApplicationType,
                string.Join(',', variant.PricingUnits.Select(unit => (int)unit)),
                string.Join(',', variant.ServiceDateBases.Select(basis => (int)basis)))),
            variants.Select(variant => (
                variant.Code,
                variant.Profile.Value,
                variant.ApplicationType.Value,
                string.Join(',', variant.PricingUnits.Select(unit => unit.Value)),
                string.Join(',', variant.ServiceDateBases.Select(basis => basis.Value)))));
        Assert.All(variants, variant =>
        {
            Assert.Equal("Flight Departure", variant.ServiceDateBases.Single(basis => basis.Name == "FlightDeparture").Title);
            Assert.All(variant.PricingUnits, unit => Assert.StartsWith("Per ", unit.Title));
        });
        Assert.Equal("Assisted Travel", variants.Single(variant => variant.Code == "A15").Profile.Title);
    }

    [Fact]
    public async Task V122_V02_the_variant_discovery_is_filtered_by_profile()
    {
        var sizes = new Dictionary<AncillaryProfile, string>
        {
            [AncillaryProfile.Baggage] = "A01,A02,A03,A04,A05,A06",
            [AncillaryProfile.Seat] = "A07,A08,A09",
            [AncillaryProfile.Upgrade] = "A10",
            [AncillaryProfile.Meal] = "A11,A12",
            [AncillaryProfile.Pet] = "A13,A14",
            [AncillaryProfile.AssistedTravel] = "A15,A16,A17,A18,A19",
            [AncillaryProfile.AirportService] = "A20,A21,A22",
            [AncillaryProfile.Priority] = "A23",
            [AncillaryProfile.Connectivity] = "A24"
        };

        Assert.Equal(Enum.GetValues<AncillaryProfile>().Order(), sizes.Keys.Order());

        foreach (var (profile, codes) in sizes)
        {
            var variants = await RequestAsync(scope => scope.GetVariants.ExecuteAsync(new BackofficeGetAncillaryVariantsQuery { Profile = profile }));

            Assert.Equal(codes, string.Join(',', variants.Select(variant => variant.Code)));
            Assert.All(variants, variant => Assert.Equal(profile.ToString(), variant.Profile.Name));
        }

        Assert.Empty(await RequestAsync(scope => scope.GetVariants.ExecuteAsync(new BackofficeGetAncillaryVariantsQuery { Profile = (AncillaryProfile)99 })));
    }

    [Fact]
    public async Task V122_V03_every_advertised_unit_and_date_basis_is_definable_and_an_unadvertised_one_is_refused()
    {
        var airlineId = _database.NextAirlineId();
        var local = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var external = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider));
        var variants = await RequestAsync(scope => scope.GetVariants.ExecuteAsync(new BackofficeGetAncillaryVariantsQuery()));
        var defined = 0;

        foreach (var advertised in variants)
        {
            var variant = V122Catalog.Case(advertised.Code);
            var command = V122Catalog.Define(variant, airlineId, V122Catalog.NeedsExternalSupplier(variant) ? external : local);
            var units = advertised.PricingUnits.Select(unit => (PricingUnit)unit.Value).ToList();
            var bases = advertised.ServiceDateBases.Select(basis => (ServiceDateBasis)basis.Value).ToList();

            foreach (var unit in units)
            foreach (var basis in bases)
            {
                var draft = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with
                {
                    ServiceDefinitionRef = $"V{++defined:000}_{variant.Code}",
                    PricingUnit = unit,
                    ServiceDateBasis = basis
                }));
                var detail = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(draft.Id));

                Assert.Equal((variant.Code, unit.ToString(), basis.ToString()), (detail.VariantCode, detail.PricingUnit!.Name, detail.ServiceDateBasis!.Name));
            }

            var otherUnit = Enum.GetValues<PricingUnit>().First(unit => !units.Contains(unit));
            var otherBasis = Enum.GetValues<ServiceDateBasis>().First(basis => !bases.Contains(basis));

            await BusinessAssert.ThrowsAsync(16202, 422, () => RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with
            {
                ServiceDefinitionRef = $"VU_{variant.Code}",
                PricingUnit = otherUnit
            })));
            await BusinessAssert.ThrowsAsync(16202, 422, () => RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with
            {
                ServiceDefinitionRef = $"VB_{variant.Code}",
                ServiceDateBasis = otherBasis
            })));
        }

        Assert.Equal(37, defined);
        Assert.Equal(
            37,
            (await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, PageSize = 1 }))).TotalCount);
    }
}
