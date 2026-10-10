using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record ShoppingFilter
    {
        public static readonly ShoppingFilter None = new();

        public IReadOnlyList<AncillaryProfile>? Profiles { get; init; }

        public IReadOnlyList<string>? VariantCodes { get; init; }

        public IReadOnlyList<string>? ServiceDefinitionRefs { get; init; }

        public IReadOnlyList<string>? FlightRefs { get; init; }

        public IReadOnlyList<string>? TravellerRefs { get; init; }

        public bool IncludeNonSellable { get; init; }
    }
}
