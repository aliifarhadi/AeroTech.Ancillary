using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryProducts;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1AncillaryProductPersistenceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    public async Task InitializeAsync() => await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P1_C02_UniqueIndexRejectsASecondProductWithTheSameReference()
    {
        await _harness.DefineAsync(Phase1Commands.ProductX(Airline));

        await using var scope = _harness.NewScope();

        await scope.Products.AddAsync(NewProductX(await SubCodeAsync(scope)));

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
    }

    [Fact]
    public async Task P1_C19_UniqueIndexRejectsASecondDraftOfTheSameProduct()
    {
        var first = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));

        await _harness.ReviseProductAsync(first.Id);

        await using var scope = _harness.NewScope();

        var source = (await scope.Products.GetAsync(first.Id))!;

        await scope.Products.AddAsync(source.Revise(database.Ids.NewId(), 3, _harness.Clock.Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task P1_C20_ActivatingADraftRetiresThePreviousVersionInTheSameSave(bool previousIsSuspended)
    {
        var first = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        var second = await _harness.ReviseProductAsync(first.Id);

        if (previousIsSuspended)
            await _harness.SuspendProductAsync(first.Id);

        _harness.Clock.Now = _harness.Clock.Now.AddDays(1);

        var result = await _harness.ActivateProductAsync(second.Id);

        Assert.Equal((second.Id, 2, AncillaryProductStatus.Active, (long?)first.Id), (result.Id, result.Version, result.Status, result.RetiredVersionId));

        var versions = await VersionsAsync();
        var readModels = await _harness.RunAsync(scope => scope.Query.AncillaryProducts
            .Where(row => row.OwnerAirlineId == Airline)
            .OrderBy(row => row.Version)
            .Select(row => new { row.Version, row.Status, row.RetiredAt, row.ActivatedAt })
            .ToListAsync());

        Assert.Equal([(1, AncillaryProductStatus.Retired), (2, AncillaryProductStatus.Active)], versions.Select(version => (version.Version, version.Status)));
        Assert.Equal((_harness.Clock.Now, _harness.Clock.Now), (versions[0].RetiredAt, versions[1].ActivatedAt));
        Assert.Equal(
            [(1, AncillaryProductStatus.Retired, (DateTimeOffset?)_harness.Clock.Now), (2, AncillaryProductStatus.Active, null)],
            readModels.Select(row => (row.Version, row.Status, row.RetiredAt)));
    }

    [Fact]
    public async Task P1_C20_UniqueIndexAllowsOnlyOneOfferedVersion()
    {
        var first = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        var second = await _harness.ReviseProductAsync(first.Id);

        await using var scope = _harness.NewScope();

        var draft = (await scope.Products.GetAsync(second.Id))!;

        draft.Activate(await SubCodeAsync(scope), _harness.Clock.Now);

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
        Assert.Equal([(1, AncillaryProductStatus.Active), (2, AncillaryProductStatus.Draft)], (await VersionsAsync()).Select(version => (version.Version, version.Status)));
    }

    [Fact]
    public async Task P1_C22_LoserOfTwoSimultaneousActivationsLeavesNoChange()
    {
        var draft = await _harness.DefineAsync(Phase1Commands.ProductX(Airline));
        var command = new BackofficeActivateAncillaryProductCommand(draft.Id);
        var winnerAt = _harness.Clock.Now.AddHours(1);

        _harness.Clock.Now = winnerAt;

        await using var losing = _harness.NewScope();

        var gate = new GatedAncillaryProductRepository(losing.Products);
        var loser = new ActivateAncillaryProductService(gate, losing.SubCodes, losing.ProductSynchronizer, losing.UnitOfWork, _harness.Clock)
            .ActivateAsync(command);

        await gate.Loaded;

        var winner = await _harness.ActivateProductAsync(draft.Id);
        var afterWinner = await RowAsync(draft.Id);

        _harness.Clock.Now = winnerAt.AddHours(1);
        gate.Release();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => loser);

        var afterLoser = await RowAsync(draft.Id);
        var readModel = await _harness.GetProductAsync(draft.Id);

        Assert.Equal(AncillaryProductStatus.Active, winner.Status);
        Assert.Equal((AncillaryProductStatus.Active, (DateTimeOffset?)winnerAt), (afterLoser.Status, afterLoser.ActivatedAt));
        Assert.Equal(afterWinner, afterLoser);
        Assert.Equal(("Active", winnerAt), (readModel.Status.Name, readModel.ActivatedAt));
        Assert.Equal([(1, AncillaryProductStatus.Active)], (await VersionsAsync()).Select(version => (version.Version, version.Status)));
    }

    [Fact]
    public async Task P1_C22_ExactlyOneOfTwoParallelActivationsSucceeds()
    {
        var draft = await _harness.DefineAsync(Phase1Commands.ProductX(Airline));

        var outcomes = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() => TryActivateAsync(draft.Id))));

        Assert.Equal(1, outcomes.Count(succeeded => succeeded));
        Assert.Equal([(1, AncillaryProductStatus.Active)], (await VersionsAsync()).Select(version => (version.Version, version.Status)));
        Assert.Equal("Active", (await _harness.GetProductAsync(draft.Id)).Status.Name);
    }

    private async Task<bool> TryActivateAsync(long productId)
    {
        try
        {
            await _harness.ActivateProductAsync(productId);

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (BusinessException exception) when (exception.Code == 16104)
        {
            return false;
        }
    }

    private Task<List<(int Version, AncillaryProductStatus Status, DateTimeOffset? ActivatedAt, DateTimeOffset? RetiredAt)>> VersionsAsync()
        => _harness.RunAsync(async scope => (await scope.Command.AncillaryProducts
                .AsNoTracking()
                .Where(product => product.OwnerAirlineId == Airline)
                .OrderBy(product => product.Version)
                .ToListAsync())
            .Select(product => (product.Version, product.Status, product.ActivatedAt, product.RetiredAt))
            .ToList());

    private Task<(AncillaryProductStatus Status, DateTimeOffset? ActivatedAt, string RowVersion)> RowAsync(long productId)
        => _harness.RunAsync(async scope =>
        {
            var product = await scope.Command.AncillaryProducts.AsNoTracking().SingleAsync(row => row.Id == productId);

            return (product.Status, product.ActivatedAt, Convert.ToHexString(product.RowVersion));
        });

    private Task<ServiceSubCode?> SubCodeAsync(AncillaryScope scope) => scope.SubCodes.FindActiveAsync(Airline, "0CC");

    private AncillaryProduct NewProductX(ServiceSubCode? subCode)
        => AncillaryProduct.Define(
            database.Ids.NewId(),
            Airline,
            "XBAG1",
            AncillaryProductType.ExtraBaggage,
            "First extra bag 23kg",
            null,
            AncillarySalesScope.TravellerBound,
            new QuantityPolicy(AncillaryQuantityUnit.Piece, 1, 1),
            AncillaryDocumentType.EmdAssociated,
            "0CC",
            "C",
            new SalesTerms(false, null, null, null, null),
            AncillaryInventoryControl.Unlimited,
            new BaggageDetail(1, 23m, AncillaryWeightUnit.Kg),
            null,
            subCode,
            _harness.Clock.Now);
}
