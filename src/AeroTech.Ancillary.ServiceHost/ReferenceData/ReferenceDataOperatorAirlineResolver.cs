using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataOperatorAirlineResolver : IOperatorAirlineResolver
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataOperatorAirlineResolver(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public Task<long?> FindHomeAirlineIdAsync(CancellationToken cancellationToken = default)
            => _referenceData.OperatorSettings
                .AsNoTracking()
                .Where(settings => settings.ScopeKey == OperatorScopeKey.HomeOperator)
                .Select(settings => (long?)settings.HomeAirlineId)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
