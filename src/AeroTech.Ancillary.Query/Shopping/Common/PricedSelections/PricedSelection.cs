using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed record PricedSelection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; init; } = CurrentSchemaVersion;

        public required ShoppingSourceKind SourceKind { get; init; }

        public required long OrderId { get; init; }

        public required int OrderCommercialVersion { get; init; }

        public required string SourceVersion { get; init; }

        public required AncillarySalesScope Scope { get; init; }

        public required SelectionLineKind Kind { get; init; }

        public string? OfferItemRef { get; init; }

        public string? SeatMapSelectionId { get; init; }

        public AncillarySelection? Selection { get; init; }

        public PricedSeat? Seat { get; init; }

        public required ServiceCoverageScope CoverageScope { get; init; }

        public required IReadOnlyList<string> TravellerRefs { get; init; }

        public required IReadOnlyList<string> FlightRefs { get; init; }

        public required IReadOnlyList<string> PortionRefs { get; init; }

        public string? ServiceDefinitionRef { get; init; }

        public long? DefinitionVersionId { get; init; }

        public long? ProvisionId { get; init; }

        public long? PricingRevisionId { get; init; }

        public required int CurrencyId { get; init; }

        public required decimal Total { get; init; }

        public int? Units { get; init; }
    }
}
