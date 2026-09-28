using AeroTech.Ancillary.ReferenceData.AirInfo;
using AeroTech.Ancillary.ReferenceData.AirInfo.Wire;
using AeroTech.Ancillary.ReferenceData.Configuration;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using Microsoft.Extensions.Options;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class CitySyncer : ReferenceSyncerBase<CityReadModel, CityDto, int>
    {
        private readonly IAirInfoClient _client;
        private readonly string _primaryLanguage;

        public CitySyncer(ReferenceDbContext db, IAirInfoClient client, IOptions<ReferenceDataOptions> options, TimeProvider timeProvider)
            : base(db, timeProvider)
        {
            _client = client;
            _primaryLanguage = options.Value.PrimaryLanguage;
        }

        protected override string Resource => "Cities";

        protected override Task<List<CityDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetCitiesAsync(modifiedAfter, cancellationToken);

        protected override CityReadModel CreateNew(CityDto dto) => new()
        {
            Id = dto.Id,
            IataCode = dto.IataCode,
            DisplayName = DisplayNameSelector.Pick(dto.DisplayNames, _primaryLanguage),
            CountryId = dto.State?.Country?.Id,
            LastUpdateTime = dto.LastUpdateTime
        };

        protected override void ApplyChanges(CityDto dto, CityReadModel model)
        {
            model.IataCode = dto.IataCode;
            model.DisplayName = DisplayNameSelector.Pick(dto.DisplayNames, _primaryLanguage);
            model.CountryId = dto.State?.Country?.Id;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
