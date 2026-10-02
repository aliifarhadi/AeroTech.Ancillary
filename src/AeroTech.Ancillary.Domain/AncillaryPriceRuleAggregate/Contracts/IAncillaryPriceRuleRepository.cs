namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts
{
    public interface IAncillaryPriceRuleRepository
    {
        Task AddAsync(AncillaryPriceRule rule, CancellationToken cancellationToken = default);

        Task<AncillaryPriceRule?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<bool> ActivePriorityExistsAsync(
            int ownerAirlineId,
            string productRef,
            int currencyId,
            int priority,
            long exceptRuleId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AncillaryPriceRule>> ListActiveAsync(int currencyId, CancellationToken cancellationToken = default);
    }
}
