namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface IInventoryConfigurationReader
    {
        Task<IReadOnlyList<InventoryConfiguration>> ListAsync(
            int ownerAirlineId,
            IReadOnlyCollection<string> serviceDefinitionRefs,
            IReadOnlyCollection<long> flightIds,
            DateTimeOffset evaluatedAtUtc,
            CancellationToken cancellationToken = default);
    }
}
