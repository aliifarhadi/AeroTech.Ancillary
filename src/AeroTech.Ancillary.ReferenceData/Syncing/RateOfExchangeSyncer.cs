using AeroTech.Ancillary.ReferenceData.AirPrice;
using AeroTech.Ancillary.ReferenceData.AirPrice.Wire;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class RateOfExchangeSyncer : ReferenceSyncerBase<RateOfExchangeReadModel, RateOfExchangeDto, long>
    {
        private readonly IAirPriceClient _client;

        public RateOfExchangeSyncer(ReferenceDbContext db, IAirPriceClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "RateOfExchanges";

        protected override bool FetchesCompleteSet => true;

        protected override Task<List<RateOfExchangeDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetRateOfExchangesAsync(cancellationToken);

        protected override RateOfExchangeReadModel CreateNew(RateOfExchangeDto dto)
        {
            var model = new RateOfExchangeReadModel { Id = dto.Id };
            ApplyChanges(dto, model);
            return model;
        }

        protected override void ApplyChanges(RateOfExchangeDto dto, RateOfExchangeReadModel model)
        {
            model.RateOfExchangeTableId = dto.RateOfExchangeTableId;
            model.TableCode = dto.TableCode;
            model.Application = dto.Application;
            model.FromCurrencyId = dto.FromCurrencyId;
            model.ToCurrencyId = dto.ToCurrencyId;
            model.Rate = dto.Rate;
            model.EffectiveFrom = dto.EffectiveFrom;
            model.EffectiveTo = dto.EffectiveTo;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
