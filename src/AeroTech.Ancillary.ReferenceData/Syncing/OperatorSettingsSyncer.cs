using AeroTech.Ancillary.ReferenceData.Core;
using AeroTech.Ancillary.ReferenceData.Core.Wire;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class OperatorSettingsSyncer : ReferenceSyncerBase<OperatorSettingsReadModel, OperatorSettingsDto, long>
    {
        private readonly ICoreClient _client;

        public OperatorSettingsSyncer(ReferenceDbContext db, ICoreClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "OperatorSettings";

        protected override Task<List<OperatorSettingsDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetOperatorSettingsAsync(modifiedAfter, cancellationToken);

        protected override OperatorSettingsReadModel CreateNew(OperatorSettingsDto dto)
        {
            var model = new OperatorSettingsReadModel { Id = dto.Id };
            ApplyChanges(dto, model);
            return model;
        }

        protected override void ApplyChanges(OperatorSettingsDto dto, OperatorSettingsReadModel model)
        {
            model.ScopeKey = dto.ScopeKey;
            model.HomeAirlineId = dto.HomeAirlineId;
            model.DefaultCurrencyId = dto.DefaultCurrencyId;
            model.DefaultLanguageCode = dto.DefaultLanguageCode;
            model.DefaultTimeZoneId = dto.DefaultTimeZoneId;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
