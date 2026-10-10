using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record TravellerFlightBaggageFacts
    {
        public required string TravellerRef { get; init; }

        public required string FlightRef { get; init; }

        public required string PortionRef { get; init; }

        public int? CheckedPieces { get; init; }

        public decimal? CheckedWeight { get; init; }

        public WeightUnit? CheckedWeightUnit { get; init; }

        public int? CabinPieces { get; init; }

        public decimal? CabinWeight { get; init; }

        public WeightUnit? CabinWeightUnit { get; init; }

        public required FactEvidence SourceCompleteness { get; init; }

        public string? FareBaggageEntitlementRef { get; init; }
    }
}
