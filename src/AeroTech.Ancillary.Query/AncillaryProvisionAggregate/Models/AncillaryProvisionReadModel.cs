using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionReadModel
    {
        public long Id { get; set; }

        public long ServiceDefinitionId { get; set; }

        public int Sequence { get; set; }

        public ProvisionStatus Status { get; set; }

        public DateTimeOffset? SalesEffectiveFrom { get; set; }

        public DateTimeOffset? SalesDiscontinueAt { get; set; }

        public ServiceCoverageScope CoverageScope { get; set; }

        public AncillaryQuantityUnit QuantityUnit { get; set; }

        public int MinQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public ProvisionApplicationType ApplicationType { get; set; }

        public CommercialDisposition Disposition { get; set; }

        public bool DocumentRequired { get; set; }

        public bool BookingRequired { get; set; }

        public int? FeeCurrencyId { get; set; }

        public FeeApplicationUnit? FeeApplicationUnit { get; set; }

        public ReissueRefundPolicy ReissueRefund { get; set; }

        public FormOfRefund? FormOfRefund { get; set; }

        public bool Commissionable { get; set; }

        public bool InterlineSettlement { get; set; }

        public bool MustCheckAvailability { get; set; }

        public string FulfillmentProviderKey { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
