using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record ExistingAncillaryServiceFacts
    {
        public required string ServiceRef { get; init; }

        public long? OrderServiceId { get; init; }

        public string? CountingFamilyCode { get; init; }

        public required string TravellerRef { get; init; }

        public string? PortionRef { get; init; }

        public required IReadOnlyList<string> FlightRefs { get; init; }

        public DateOnly? ServiceDate { get; init; }

        public required int Quantity { get; init; }

        public decimal? ConsumptionUnits { get; init; }

        public required ExistingServiceCommercialState CommercialState { get; init; }

        public string? DocumentState { get; init; }

        public string? EvidenceSourceRef { get; init; }
    }
}
