using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record AncillaryShoppingContext
    {
        public const int CurrentSchemaVersion = 1;

        public int ContextSchemaVersion { get; init; } = CurrentSchemaVersion;

        public required int OwnerAirlineId { get; init; }

        public required long PointOfSaleId { get; init; }

        public long? CustomerId { get; init; }

        public CustomerType? CustomerType { get; init; }

        public required ShoppingStage ShoppingStage { get; init; }

        public required DateTimeOffset EvaluatedAtUtc { get; init; }

        public required int CurrencyId { get; init; }

        public required SourceIdentityContext SourceIdentity { get; init; }

        public required IReadOnlyList<ShoppingTraveller> Travellers { get; init; }

        public required IReadOnlyList<ShoppingPortion> Portions { get; init; }

        public required IReadOnlyList<ShoppingFlight> Flights { get; init; }

        public IReadOnlyList<TravellerFareFacts> TravellerFareFacts { get; init; } = [];

        public IReadOnlyList<TravellerFlightBaggageFacts> TravellerBaggageFacts { get; init; } = [];

        public IReadOnlyList<FareBenefitFacts> FareEntitlementFacts { get; init; } = [];

        public IReadOnlyList<ExistingAncillaryServiceFacts> ExistingServiceFacts { get; init; } = [];

        public IReadOnlyList<VerifiedUsageEvidence> UsageEvidence { get; init; } = [];

        public required FactCompleteness CoverageCompleteness { get; init; }
    }
}
