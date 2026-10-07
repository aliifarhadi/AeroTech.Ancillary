using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionRoutePairReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public RoutePairDirection Direction { get; set; }
    }
}
