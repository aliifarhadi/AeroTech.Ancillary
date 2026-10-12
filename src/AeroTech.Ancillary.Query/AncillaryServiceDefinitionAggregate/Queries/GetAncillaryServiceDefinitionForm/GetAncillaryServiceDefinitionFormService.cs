using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public sealed class GetAncillaryServiceDefinitionFormService : IGetAncillaryServiceDefinitionFormService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IAncillaryServiceDefinitionRepository _definitions;

        public GetAncillaryServiceDefinitionFormService(AncillaryQueryDbContext dbContext, IAncillaryServiceDefinitionRepository definitions)
        {
            _dbContext = dbContext;
            _definitions = definitions;
        }

        public async Task<BackofficeServiceDefinitionFormDto<TSpecification>> ExecuteAsync<TSpecification>(
            long serviceDefinitionId,
            AncillaryProfile profile,
            Func<AncillaryServiceDefinition, TSpecification> specification,
            CancellationToken cancellationToken = default)
            where TSpecification : class
        {
            var definition = await _definitions.GetAsync(serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            if (!definition.IsClassified)
                throw ExceptionFactory.ServiceDefinitionNotClassified();

            if (definition.Profile != profile)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(definition.Profile, profile);

            var supplierName = await _dbContext.AncillaryServiceDefinitions
                .AsNoTracking()
                .Where(row => row.Id == definition.Id)
                .Select(row => row.SupplierName)
                .FirstAsync(cancellationToken);

            return AncillaryServiceDefinitionFormMapper.ToBackofficeForm(definition, supplierName, specification(definition));
        }
    }
}
