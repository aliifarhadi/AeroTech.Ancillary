namespace AeroTech.Ancillary.ReferenceData.ReadModels
{
    public sealed class AirFareReadModel : IReferenceReadModel<long>
    {
        public long Id { get; set; }
        public long FareNumber { get; set; }
        public long? FareFamilyId { get; set; }
        public string? FareFamilyTitle { get; set; }
        public int? CabinClassId { get; set; }
        public long? RbdId { get; set; }
        public string? FareBasis { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
