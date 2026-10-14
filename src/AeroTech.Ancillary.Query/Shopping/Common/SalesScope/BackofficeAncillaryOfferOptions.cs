namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class BackofficeAncillaryOfferOptions
    {
        public string? CustomerId { get; set; }

        public long? CountryId { get; set; }

        public long? PointOfSaleId { get; set; }

        public DateTimeOffset? SalesDate { get; set; }

        public int? PreferredCurrencyId { get; set; }
    }
}
