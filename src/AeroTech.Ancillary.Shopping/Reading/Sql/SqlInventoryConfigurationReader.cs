using AeroTech.Ancillary.Persistence;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlInventoryConfigurationReader : IInventoryConfigurationReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlInventoryConfigurationReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<InventoryConfiguration>> ListAsync(
            int ownerAirlineId,
            IReadOnlyCollection<string> serviceDefinitionRefs,
            IReadOnlyCollection<long> flightIds,
            DateTimeOffset evaluatedAtUtc,
            CancellationToken cancellationToken = default)
        {
            if (serviceDefinitionRefs.Count == 0)
                return [];

            var policies = await _dbContext.AncillaryInventoryPolicies
                .AsNoTracking()
                .AsSplitQuery()
                .WithChildren(_dbContext)
                .Where(policy => policy.OwnerAirlineId == ownerAirlineId
                                 && serviceDefinitionRefs.Contains(policy.ServiceDefinitionRef)
                                 && policy.Status != InventoryRecordStatus.Retired)
                .OrderBy(policy => policy.Id)
                .ToListAsync(cancellationToken);
            var countResources = policies.Where(policy => policy.CountConsumption is not null).Select(policy => policy.CountConsumption!.ResourceId).Distinct().ToList();
            var weightResources = policies.Where(policy => policy.WeightConsumption is not null).Select(policy => policy.WeightConsumption!.WeightResourceId).Distinct().ToList();
            var facilities = policies.Where(policy => policy.SlotConsumption is not null).Select(policy => policy.SlotConsumption!.FacilityId).Distinct().ToList();
            var counts = countResources.Count == 0 || flightIds.Count == 0
                ? []
                : await _dbContext.FlightCountInventories
                    .AsNoTracking()
                    .Where(source => source.OwnerAirlineId == ownerAirlineId
                                     && flightIds.Contains(source.FlightId)
                                     && countResources.Contains(source.ResourceId)
                                     && source.Status != InventoryRecordStatus.Retired)
                    .Select(source => new FlightInventorySource(InventoryResourceKind.FlightCount, source.FlightId, source.ResourceId, source.Status, source.ClosedForSale))
                    .ToListAsync(cancellationToken);
            var weights = weightResources.Count == 0 || flightIds.Count == 0
                ? []
                : await _dbContext.FlightWeightInventories
                    .AsNoTracking()
                    .Where(source => source.OwnerAirlineId == ownerAirlineId
                                     && flightIds.Contains(source.FlightId)
                                     && weightResources.Contains(source.WeightResourceId)
                                     && source.Status != InventoryRecordStatus.Retired)
                    .Select(source => new FlightInventorySource(InventoryResourceKind.FlightWeight, source.FlightId, source.WeightResourceId, source.Status, source.ClosedForSale))
                    .ToListAsync(cancellationToken);
            var slots = facilities.Count == 0
                ? []
                : await _dbContext.AirportSlotInventories
                    .AsNoTracking()
                    .Where(source => source.OwnerAirlineId == ownerAirlineId
                                     && facilities.Contains(source.FacilityId)
                                     && source.EndUtc > evaluatedAtUtc
                                     && source.Status != InventoryRecordStatus.Retired)
                    .Select(source => new SlotInventorySource(source.FacilityId, source.AirportId, source.StartUtc, source.EndUtc, source.Status, source.ClosedForSale))
                    .ToListAsync(cancellationToken);

            return policies
                .Select(policy => new InventoryConfiguration(
                    policy,
                    counts
                        .Where(source => source.ResourceId == policy.CountConsumption?.ResourceId)
                        .Concat(weights.Where(source => source.ResourceId == policy.WeightConsumption?.WeightResourceId))
                        .OrderBy(source => source.FlightId)
                        .ThenBy(source => source.Kind)
                        .ToList(),
                    slots.Where(source => source.FacilityId == policy.SlotConsumption?.FacilityId).OrderBy(source => source.StartUtc).ToList()))
                .ToList();
        }
    }
}
