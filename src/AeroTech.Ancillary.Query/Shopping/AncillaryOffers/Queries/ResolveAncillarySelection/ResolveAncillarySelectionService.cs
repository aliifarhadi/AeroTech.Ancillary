using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Resolution;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection
{
    public sealed class ResolveAncillarySelectionService : IResolveAncillarySelectionService
    {
        private readonly ISelectionResolver _resolver;

        public ResolveAncillarySelectionService(ISelectionResolver resolver) => _resolver = resolver;

        public Task<IReadOnlyList<ResolvedSelection>> ExecuteAsync(string? selectionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(selectionId))
                throw ExceptionFactory.ShoppingContextIsInvalid("selectionId is required");

            return _resolver.ResolveAsync(selectionId, cancellationToken);
        }
    }
}
