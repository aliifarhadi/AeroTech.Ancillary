using AeroTech.Ancillary.ReferenceData.AirPrice;
using AeroTech.Ancillary.ReferenceData.AirPrice.Wire;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class PointOfSaleSyncer : ReferenceSyncerBase<PointOfSaleReadModel, PointOfSaleDto, long>
    {
        private readonly IAirPriceClient _client;

        public PointOfSaleSyncer(ReferenceDbContext db, IAirPriceClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "PointOfSales";

        protected override bool FetchesCompleteSet => true;

        protected override Task<List<PointOfSaleDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetPointOfSalesAsync(cancellationToken);

        protected override PointOfSaleReadModel CreateNew(PointOfSaleDto dto)
        {
            var model = new PointOfSaleReadModel { Id = dto.Id };
            ApplyChanges(dto, model);
            return model;
        }

        protected override void ApplyChanges(PointOfSaleDto dto, PointOfSaleReadModel model)
        {
            model.Number = dto.Number;
            model.Title = dto.Title;
            model.LastUpdateTime = dto.LastUpdateTime;
        }

        protected override async Task SyncChildrenAsync(PointOfSaleDto dto, CancellationToken cancellationToken)
        {
            var incoming = dto.Conditions ?? new List<PointOfSaleConditionDto>();
            var incomingIds = incoming.Select(condition => condition.Id).ToHashSet();
            var existing = await Db.PointOfSaleConditions
                .Where(condition => condition.PointOfSaleId == dto.Id)
                .ToListAsync(cancellationToken);
            var existingById = existing.ToDictionary(condition => condition.Id);

            foreach (var condition in incoming)
            {
                if (!existingById.TryGetValue(condition.Id, out var current))
                {
                    current = new PointOfSaleConditionReadModel { Id = condition.Id, PointOfSaleId = dto.Id };
                    await Db.PointOfSaleConditions.AddAsync(current, cancellationToken);
                }

                current.CountryIsInclude = condition.CountryIsInclude;
                current.CountryIds = condition.CountryIds?.ToList() ?? [];
                current.AirlineOfficeIsInclude = condition.AirlineOfficeIsInclude;
                current.AirlineOfficeIds = condition.AirlineOfficeIds?.ToList() ?? [];
                current.TravelAgencyOfficeIsInclude = condition.TravelAgencyOfficeIsInclude;
                current.TravelAgencyOfficeIds = condition.TravelAgencyOfficeIds?.ToList() ?? [];
                current.ChannelIsInclude = condition.ChannelIsInclude;
                current.Channels = condition.Channels?.ToList() ?? [];
            }

            foreach (var current in existing)
                if (!incomingIds.Contains(current.Id))
                    Db.PointOfSaleConditions.Remove(current);
        }

        protected override async Task RemoveChildrenAsync(long id, CancellationToken cancellationToken)
        {
            var conditions = await Db.PointOfSaleConditions
                .Where(condition => condition.PointOfSaleId == id)
                .ToListAsync(cancellationToken);

            Db.PointOfSaleConditions.RemoveRange(conditions);
        }
    }
}
