using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionAirFareTypeReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public AirFareType AirFareType { get; set; }
    }
}
