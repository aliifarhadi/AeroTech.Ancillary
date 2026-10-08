using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionCustomerTypeReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public CustomerType CustomerType { get; set; }
    }
}
