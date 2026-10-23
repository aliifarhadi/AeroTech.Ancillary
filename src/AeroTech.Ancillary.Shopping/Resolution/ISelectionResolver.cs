namespace AeroTech.Ancillary.Shopping.Resolution
{
    public interface ISelectionResolver
    {
        Task<IReadOnlyList<ResolvedSelection>> ResolveAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken = default);
    }
}
