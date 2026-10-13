namespace AeroTech.Ancillary.ReferenceData.ReadModels
{
    public sealed class RateOfExchangeReadModel : IReferenceReadModel<long>
    {
        public long Id { get; set; }
        public string RateOfExchangeTableId { get; set; } = default!;
        public string TableCode { get; set; } = default!;
        public int Application { get; set; }
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal Rate { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
