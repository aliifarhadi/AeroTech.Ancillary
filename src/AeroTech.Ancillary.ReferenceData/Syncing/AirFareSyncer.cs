using AeroTech.Ancillary.ReferenceData.AirPrice;
using AeroTech.Ancillary.ReferenceData.AirPrice.Wire;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;

namespace AeroTech.Ancillary.ReferenceData.Syncing
{
    public sealed class AirFareSyncer : ReferenceSyncerBase<AirFareReadModel, AirFareDto, long>
    {
        private readonly IAirPriceClient _client;

        public AirFareSyncer(ReferenceDbContext db, IAirPriceClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "AirFares";

        protected override bool FetchesCompleteSet => true;

        protected override Task<List<AirFareDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetAirFaresAsync(cancellationToken);

        protected override AirFareReadModel CreateNew(AirFareDto dto)
        {
            var model = new AirFareReadModel { Id = dto.Id };
            ApplyChanges(dto, model);
            return model;
        }

        protected override void ApplyChanges(AirFareDto dto, AirFareReadModel model)
        {
            model.FareNumber = dto.FareNumber;
            model.FareFamilyId = dto.FareFamily?.FareFamilyId;
            model.FareFamilyTitle = dto.FareFamily?.FareFamily;
            model.CabinClassId = dto.CabinClass?.CabinClassId;
            model.RbdId = dto.BookingClass?.RbdId;
            model.FareBasis = dto.FareBasis;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
