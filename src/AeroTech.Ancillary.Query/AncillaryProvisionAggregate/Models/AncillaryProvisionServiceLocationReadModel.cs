using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionServiceLocationReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public ServiceLocationType LocationType { get; set; }

        public int LocationId { get; set; }
    }
}
