using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models
{
    public sealed class AncillaryPriceComponentReadModel
    {
        public long Id { get; set; }

        public long AncillaryPricingId { get; set; }

        public long AncillaryPricingRateId { get; set; }

        public AncillaryPriceLineCategory Category { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public int? CountryId { get; set; }

        public int? StationAirportId { get; set; }

        public decimal Amount { get; set; }

        public int CurrencyId { get; set; }

        public FeeApplicationUnit? FeeApplicationUnit { get; set; }

        public bool? TaxIncludedInSource { get; set; }
    }
}
