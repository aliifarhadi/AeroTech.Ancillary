using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class TestPointOfSaleConditions : IPointOfSaleConditionReference
{
    public Task<PointOfSaleConditionSet> GetConditionSetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new PointOfSaleConditionSet([ShoppingLab.Pos], [new PointOfSaleCondition { PointOfSaleId = ShoppingLab.Pos }]));
}
