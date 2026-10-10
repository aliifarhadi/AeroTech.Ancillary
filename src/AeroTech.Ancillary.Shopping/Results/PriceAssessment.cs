using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record PriceAssessment
    {
        public required PriceOrigin Origin { get; init; }

        public required PriceAssessmentStatus Status { get; init; }

        public int? CurrencyId { get; init; }

        public long? PricingRevisionId { get; init; }

        public long? PricingRateId { get; init; }

        public PricingUnit? PricingUnit { get; init; }

        public decimal? BaseAmount { get; init; }

        public IReadOnlyList<PriceComponentLine> AddedTaxLines { get; init; } = [];

        public IReadOnlyList<PriceComponentLine> IncludedTaxLines { get; init; } = [];

        public IReadOnlyList<PriceComponentLine> AppliedUnitFeeLines { get; init; } = [];

        public IReadOnlyList<PriceComponentLine> UnappliedFeeLines { get; init; } = [];

        public decimal? CompleteUnitTotal { get; init; }

        public decimal? RequestedQuantityTotal { get; init; }

        public bool IsOrderLevelTotalComplete { get; init; }

        public string? QuoteProviderKey { get; init; }

        public string? ExternalQuoteRef { get; init; }

        public DateTimeOffset? QuoteExpiresAtUtc { get; init; }

        public IReadOnlyList<string> ReasonCodes { get; init; } = [];
    }

    public sealed record PriceComponentLine(
        AncillaryPriceLineCategory Category,
        string? Code,
        int? CountryId,
        int? StationAirportId,
        decimal Amount,
        int CurrencyId,
        TaxTreatment? TaxTreatment,
        FeeApplicationUnit? FeeApplicationUnit);
}
