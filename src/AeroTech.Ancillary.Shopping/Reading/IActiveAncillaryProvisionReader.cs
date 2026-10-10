using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface IActiveAncillaryProvisionReader
    {
        Task<IReadOnlyList<AncillaryProvision>> ListActiveAsync(
            IReadOnlyCollection<long> serviceDefinitionIds,
            long pointOfSaleId,
            CancellationToken cancellationToken = default);
    }
}
