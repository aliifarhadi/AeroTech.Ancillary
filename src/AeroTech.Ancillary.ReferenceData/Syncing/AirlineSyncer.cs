using AeroTech.Ancillary.ReferenceData.AirInfo;
using AeroTech.Ancillary.ReferenceData.AirInfo.Wire;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class AirlineSyncer : ReferenceSyncerBase<AirlineReadModel, AirlineDto, int>
    {
        private readonly IAirInfoClient _client;

        public AirlineSyncer(ReferenceDbContext db, IAirInfoClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "Airlines";

        protected override Task<List<AirlineDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetAirlinesAsync(modifiedAfter, cancellationToken);

        protected override AirlineReadModel CreateNew(AirlineDto dto) => new()
        {
            Id = dto.Id,
            IataCode = dto.IataCode,
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            LogoUrl = dto.LogoURL,
            LastUpdateTime = dto.LastUpdateTime
        };

        protected override void ApplyChanges(AirlineDto dto, AirlineReadModel model)
        {
            model.IataCode = dto.IataCode;
            model.Name = dto.Name;
            model.DisplayName = dto.DisplayName;
            model.LogoUrl = dto.LogoURL;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
