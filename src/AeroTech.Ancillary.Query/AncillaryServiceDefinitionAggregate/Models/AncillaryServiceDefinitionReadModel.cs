using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models
{
    public sealed class AncillaryServiceDefinitionReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public long SupplierId { get; set; }

        public string SupplierName { get; set; } = default!;

        public string ServiceDefinitionRef { get; set; } = default!;

        public int Version { get; set; }

        public string ServiceTypeCode { get; set; } = default!;

        public string ServiceSubCode { get; set; } = default!;

        public ServiceSubCodeSource SubCodeSource { get; set; }

        public string GroupCode { get; set; } = default!;

        public string? SubGroupCode { get; set; }

        public string? Description1Code { get; set; }

        public string? Description2Code { get; set; }

        public string CommercialName { get; set; } = default!;

        public string? Description { get; set; }

        public AncillaryDocumentType DocumentType { get; set; }

        public string? DocumentRfic { get; set; }

        public string? DocumentRfisc { get; set; }

        public BookingMethod BookingMethod { get; set; }

        public string? BookingSsrCode { get; set; }

        public string? BookingSsimCode { get; set; }

        public DateOnly? SalesEffectiveFrom { get; set; }

        public DateOnly? SalesDiscontinueOn { get; set; }

        public ServiceDefinitionStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
