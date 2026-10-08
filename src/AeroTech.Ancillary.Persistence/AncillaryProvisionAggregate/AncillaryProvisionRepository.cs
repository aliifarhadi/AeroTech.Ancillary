using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvisionRepository : IAncillaryProvisionRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryProvisionRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryProvision provision, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryProvisions.AddAsync(provision, cancellationToken);

        public Task<AncillaryProvision?> GetAsync(long id, CancellationToken cancellationToken = default)
            => Aggregates().FirstOrDefaultAsync(provision => provision.Id == id, cancellationToken);

        public Task<AncillaryProvision?> FindActiveAtSequenceAsync(long serviceDefinitionId, int sequence, CancellationToken cancellationToken = default)
            => Aggregates().FirstOrDefaultAsync(
                provision => provision.ServiceDefinitionId == serviceDefinitionId
                             && provision.Sequence == sequence
                             && provision.Status == ProvisionStatus.Active,
                cancellationToken);

        private IQueryable<AncillaryProvision> Aggregates()
            => _dbContext.AncillaryProvisions
                .Include(provision => provision.PassengerTypes)
                .Include(provision => provision.PointsOfSale)
                .Include(provision => provision.Customers)
                .Include(provision => provision.CustomerTypes)
                .Include(provision => provision.OriginAirports)
                .Include(provision => provision.DestinationAirports)
                .Include(provision => provision.ViaAirports)
                .Include(provision => provision.RoutePairs)
                .Include(provision => provision.MarketingAirlines)
                .Include(provision => provision.OperatingAirlines)
                .Include(provision => provision.FlightNumbers)
                .Include(provision => provision.Flights)
                .Include(provision => provision.Aircraft)
                .Include(provision => provision.AirFares)
                .Include(provision => provision.AirFareTypes)
                .Include(provision => provision.FareFamilies)
                .Include(provision => provision.FareBases)
                .Include(provision => provision.CabinClasses)
                .Include(provision => provision.Rbds)
                .Include(provision => provision.TravelDates)
                .Include(provision => provision.SeasonalPeriods)
                .Include(provision => provision.BlackoutPeriods)
                .Include(provision => provision.DayTimeRestrictions)
                .Include(provision => provision.SeatNumbers)
                .Include(provision => provision.SeatCharacteristics)
                .AsSplitQuery();
    }
}
