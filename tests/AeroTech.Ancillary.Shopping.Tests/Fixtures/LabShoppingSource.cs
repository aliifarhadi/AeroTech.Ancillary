using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class LabShoppingSource : IFlightOfferShoppingSource
{
    private readonly IReadOnlyList<AncillaryShoppingContext> _contexts;

    public LabShoppingSource(IReadOnlyList<AncillaryShoppingContext> contexts) => _contexts = contexts;

    public int Loads { get; private set; }

    public Task<FlightOfferShopping> LoadAsync(string? offerId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
    {
        Loads++;

        var offer = new FlightOffer(offerId ?? string.Empty, ShoppingLab.Now, null, null, ShoppingLab.Eur, null, 0m, 0m, 0m, [], [], [], [], [], scope.Channel, null);

        return Task.FromResult(new FlightOfferShopping(
            offer,
            new FlightOfferFacts([], [], new FactCompleteness()),
            ShoppingLab.Eur,
            ShoppingLab.Now,
            _contexts.Select(context => new PointOfSaleContext(context.PointOfSaleId, context)).ToList()));
    }

    public Task<FlightOfferShopping> ContextualizeAsync(FlightOffer offer, AncillarySalesScope scope, string sourceVersion, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
