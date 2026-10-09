using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public sealed class GetAncillaryServiceDefinitionByIdService : IGetAncillaryServiceDefinitionByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IAncillaryServiceDefinitionRepository _definitions;

        public GetAncillaryServiceDefinitionByIdService(AncillaryQueryDbContext dbContext, IAncillaryServiceDefinitionRepository definitions)
        {
            _dbContext = dbContext;
            _definitions = definitions;
        }

        public async Task<BackofficeServiceDefinitionDto> ExecuteAsync(long serviceDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = await _dbContext.AncillaryServiceDefinitions
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(row => row.Id == serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            return AncillaryServiceDefinitionMapper.ToBackofficeServiceDefinition(definition, await _definitions.GetAsync(definition.Id, cancellationToken));
        }
    }
}
