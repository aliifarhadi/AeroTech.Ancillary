namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionSeatCharacteristicReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public string CharacteristicCode { get; set; } = default!;
    }
}
