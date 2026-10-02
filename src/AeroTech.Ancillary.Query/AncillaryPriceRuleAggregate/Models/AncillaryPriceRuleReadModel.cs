using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models
{
    public sealed class AncillaryPriceRuleReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public string ProductRef { get; set; } = default!;

        public int Priority { get; set; }

        public int CurrencyId { get; set; }

        public DateTimeOffset? SalesFrom { get; set; }

        public DateTimeOffset? SalesTo { get; set; }

        public DateOnly? TravelFrom { get; set; }

        public DateOnly? TravelTo { get; set; }

        public List<PassengerTypeCode>? PassengerTypes { get; set; }

        public List<int>? OriginAirportIds { get; set; }

        public List<int>? DestinationAirportIds { get; set; }

        public AncillaryPriceRuleStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
