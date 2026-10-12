namespace AeroTech.Ancillary.ReferenceData.ReadModels
{
    public sealed class PointOfSaleReadModel : IReferenceReadModel<long>
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public string Title { get; set; } = default!;
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
