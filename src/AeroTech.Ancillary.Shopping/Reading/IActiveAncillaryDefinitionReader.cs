using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface IActiveAncillaryDefinitionReader
    {
        Task<IReadOnlyList<AncillaryServiceDefinition>> ListActiveAsync(int ownerAirlineId, CancellationToken cancellationToken = default);
    }
}
