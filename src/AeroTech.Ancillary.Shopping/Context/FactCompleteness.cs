namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record FactCompleteness
    {
        public bool TravellersComplete { get; init; }

        public bool ItineraryComplete { get; init; }

        public bool FareFactsComplete { get; init; }

        public bool BaggageFactsComplete { get; init; }

        public bool FareBenefitsComplete { get; init; }

        public bool ExistingServicesComplete { get; init; }

        public string? Provenance { get; init; }
    }
}
