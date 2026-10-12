using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public interface IGetAncillaryServiceDefinitionFormService
    {
        Task<BackofficeServiceDefinitionFormDto<TSpecification>> ExecuteAsync<TSpecification>(
            long serviceDefinitionId,
            AncillaryProfile profile,
            Func<AncillaryServiceDefinition, TSpecification> specification,
            CancellationToken cancellationToken = default)
            where TSpecification : class;
    }
}
