using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Models
{
    public sealed class AncillaryProductReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public string ProductRef { get; set; } = default!;

        public int Version { get; set; }

        public AncillaryProductType Type { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public AncillarySalesScope SalesScope { get; set; }

        public AncillaryQuantityUnit QuantityUnit { get; set; }

        public int QuantityMin { get; set; }

        public int QuantityMax { get; set; }

        public AncillaryDocumentType DocumentType { get; set; }

        public string? Rfisc { get; set; }

        public string? Rfic { get; set; }

        public string ServiceTypeCode { get; set; } = default!;

        public string GroupCode { get; set; } = default!;

        public string? SubGroupCode { get; set; }

        public string? Description1Code { get; set; }

        public string? Description2Code { get; set; }

        public bool Refundable { get; set; }

        public bool? Commissionable { get; set; }

        public bool? Reusable { get; set; }

        public string? FormOfRefundCode { get; set; }

        public bool? InterlineSettlementAllowed { get; set; }

        public AncillaryInventoryControl InventoryControl { get; set; }

        public int? BaggagePieces { get; set; }

        public decimal? BaggageWeight { get; set; }

        public AncillaryWeightUnit? BaggageWeightUnit { get; set; }

        public AncillaryProductStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }
    }
}
