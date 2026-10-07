using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public sealed class GetAncillaryServiceDefinitionByIdService : IGetAncillaryServiceDefinitionByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryServiceDefinitionByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeServiceDefinitionDto> ExecuteAsync(long serviceDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = await _dbContext.AncillaryServiceDefinitions
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(row => row.Id == serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            return AncillaryServiceDefinitionMapper.ToBackofficeServiceDefinition(definition);
        }
    }
}
