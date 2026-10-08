using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public sealed class GetAncillaryProvisionByIdService : IGetAncillaryProvisionByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryProvisionByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeProvisionDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default)
        {
            var provision = await _dbContext.AncillaryProvisions
                                .AsNoTracking()
                                .FirstOrDefaultAsync(row => row.Id == provisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();
            var rows = new AncillaryProvisionConditionRows(
                await _dbContext.AncillaryProvisionPassengerTypes
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionPointsOfSale
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionCustomers
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionCustomerTypes
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionOriginAirports
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionDestinationAirports
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionViaAirports
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionRoutePairs
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionMarketingAirlines
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionOperatingAirlines
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionFlightNumbers
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionFlights
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionAircraft
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionAirFares
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionAirFareTypes
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionFareFamilies
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionFareBases
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionCabinClasses
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionRbds
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionTravelDates
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionSeasonalPeriods
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionBlackoutPeriods
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionDayTimeRestrictions
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionSeatNumbers
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken),
                await _dbContext.AncillaryProvisionSeatCharacteristics
                    .AsNoTracking()
                    .Where(row => row.AncillaryProvisionId == provisionId)
                    .OrderBy(row => row.Id)
                    .ToListAsync(cancellationToken));

            return AncillaryProvisionMapper.ToBackofficeProvision(provision, rows);
        }
    }
}
