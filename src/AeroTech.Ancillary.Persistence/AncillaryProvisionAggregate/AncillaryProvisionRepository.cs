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
                .Include(provision => provision.PassengerEligibility!)
                .ThenInclude(rule => rule.PassengerTypes)
                .Include(provision => provision.PassengerEligibility!)
                .ThenInclude(rule => rule.AgeBands)
                .Include(provision => provision.SalesRestrictions!)
                .ThenInclude(rule => rule.PointsOfSale)
                .Include(provision => provision.SalesRestrictions!)
                .ThenInclude(rule => rule.Customers)
                .Include(provision => provision.SalesRestrictions!)
                .ThenInclude(rule => rule.CustomerTypes)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.OriginAirports)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.DestinationAirports)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.ViaAirports)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.CoverageCountries)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.RoutePairs)
                .Include(provision => provision.Geography!)
                .ThenInclude(rule => rule.ServiceLocations)
                .Include(provision => provision.FlightApplication!)
                .ThenInclude(rule => rule.MarketingAirlines)
                .Include(provision => provision.FlightApplication!)
                .ThenInclude(rule => rule.OperatingAirlines)
                .Include(provision => provision.FlightApplication!)
                .ThenInclude(rule => rule.FlightNumbers)
                .Include(provision => provision.FlightApplication!)
                .ThenInclude(rule => rule.Flights)
                .Include(provision => provision.FlightApplication!)
                .ThenInclude(rule => rule.Aircraft)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.AirFares)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.AirFareTypes)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.FareFamilies)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.FareBases)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.CabinClasses)
                .Include(provision => provision.FareApplication!)
                .ThenInclude(rule => rule.Rbds)
                .Include(provision => provision.TravelDate!)
                .ThenInclude(rule => rule.PermittedPeriods)
                .Include(provision => provision.TravelDate!)
                .ThenInclude(rule => rule.BlackoutPeriods)
                .Include(provision => provision.DayTimeApplication!)
                .ThenInclude(rule => rule.Windows)
                .Include(provision => provision.AdvancePurchase)
                .Include(provision => provision.BaggageApplication)
                .Include(provision => provision.SeatApplication!)
                .ThenInclude(rule => rule.SeatNumbers)
                .Include(provision => provision.SeatApplication!)
                .ThenInclude(rule => rule.SeatCharacteristics)
                .Include(provision => provision.PetRule)
                .Include(provision => provision.AssistedTravelRule)
                .Include(provision => provision.AirportServiceRule)
                .AsSplitQuery();
    }
}
