using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models
{
    public sealed class AncillaryPricingRateReadModel
    {
        public long Id { get; set; }

        public long AncillaryPricingId { get; set; }

        public PassengerTypeCode? PassengerTypeCode { get; set; }

        public int? AgeFromInclusive { get; set; }

        public int? AgeToExclusive { get; set; }

        public int CurrencyId { get; set; }

        public decimal BaseAmount { get; set; }
    }
}
