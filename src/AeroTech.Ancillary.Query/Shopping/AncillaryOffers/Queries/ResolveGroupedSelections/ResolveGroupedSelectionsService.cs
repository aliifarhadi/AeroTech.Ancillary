using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Resolution;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public sealed class ResolveGroupedSelectionsService : IResolveGroupedSelectionsService
    {
        private readonly ISelectionResolver _resolver;

        public ResolveGroupedSelectionsService(ISelectionResolver resolver) => _resolver = resolver;

        public Task<IReadOnlyList<ResolvedSelection>> ExecuteAsync(IReadOnlyList<string>? ids, CancellationToken cancellationToken = default)
        {
            if (ids is not { Count: > 0 and <= SelectionLimits.MaximumIds })
                throw ExceptionFactory.ShoppingContextIsInvalid($"ids must hold between 1 and {SelectionLimits.MaximumIds} selection ids");

            if (ids.Any(string.IsNullOrWhiteSpace))
                throw ExceptionFactory.ShoppingContextIsInvalid("a selection id must not be blank");

            if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
                throw ExceptionFactory.ShoppingContextIsInvalid("a selection id must appear once");

            return _resolver.ResolveAsync(ids, cancellationToken);
        }
    }
}
