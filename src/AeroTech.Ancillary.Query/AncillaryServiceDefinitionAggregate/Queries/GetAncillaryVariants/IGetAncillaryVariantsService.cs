using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants
{
    public interface IGetAncillaryVariantsService
    {
        Task<IReadOnlyList<AncillaryVariantDto>> ExecuteAsync(IAncillaryVariantsQuery query, CancellationToken cancellationToken = default);
    }
}
