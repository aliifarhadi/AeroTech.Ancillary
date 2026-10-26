using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class LabSalesScopes : IBackofficeSalesScopeResolver, IOtaPanelSalesScopeResolver, IApiSalesScopeResolver
{
    private static readonly AncillarySalesScope Scope = new(ShoppingLab.Airline, default, [ShoppingLab.Pos]);

    public Task<AncillarySalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default) => Task.FromResult(Scope);

    public Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default) => Task.FromResult(Scope);
}
