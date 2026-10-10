using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record VerifiedUsageEvidence
    {
        public required PassengerUsageKey UsageSubjectKey { get; init; }

        public required decimal UnitsConsumed { get; init; }

        public required bool IsCompleteForScope { get; init; }

        public required DateTimeOffset AsOfUtc { get; init; }

        public string? SourceVersion { get; init; }

        public required string SourceAuthority { get; init; }
    }
}
