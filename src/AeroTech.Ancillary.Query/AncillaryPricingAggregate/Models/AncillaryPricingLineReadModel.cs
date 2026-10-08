using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models
{
    public sealed class AncillaryPricingLineReadModel
    {
        public long Id { get; set; }

        public long AncillaryPricingId { get; set; }

        public PassengerTypeCode? PassengerTypeCode { get; set; }

        public int? AgeFromInclusive { get; set; }

        public int? AgeToExclusive { get; set; }

        public AncillaryPriceLineCategory Category { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public int? CountryId { get; set; }

        public int? StationAirportId { get; set; }

        public decimal Amount { get; set; }
    }
}
