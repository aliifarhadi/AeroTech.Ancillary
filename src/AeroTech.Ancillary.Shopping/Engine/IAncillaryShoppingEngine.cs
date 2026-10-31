using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Shopping.Engine
{
    public interface IAncillaryShoppingEngine
    {
        Task<CanonicalAncillaryOfferResult> ShopAsync(
            AncillaryShoppingContext context,
            ShoppingFilter? filter = null,
            CancellationToken cancellationToken = default);

        Task<ISelectionEvaluator> OpenSelectionEvaluatorAsync(
            AncillaryShoppingContext context,
            IReadOnlyList<AncillaryServiceDefinition> definitions,
            CancellationToken cancellationToken = default);
    }
}
