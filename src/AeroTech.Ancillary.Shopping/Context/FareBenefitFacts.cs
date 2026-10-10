using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record FareBenefitFacts
    {
        public required string TravellerRef { get; init; }

        public required IReadOnlyList<string> FlightRefs { get; init; }

        public required string BenefitCode { get; init; }

        public required bool IncludedOrEntitled { get; init; }

        public string? SourceReference { get; init; }

        public DateTimeOffset? EvidenceTimeUtc { get; init; }

        public required FactEvidence EvidenceCompleteness { get; init; }
    }
}
