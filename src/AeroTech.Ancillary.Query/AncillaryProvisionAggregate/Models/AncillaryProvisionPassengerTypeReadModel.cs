using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionPassengerTypeReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public PassengerTypeCode PassengerTypeCode { get; set; }
    }
}
