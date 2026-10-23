using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class OfferFamilyRoutes
    {
        public const string Baggages = "Baggages";
        public const string Meals = "Meals";
        public const string Pets = "Pets";
        public const string AssistedTravel = "AssistedTravel";
        public const string AirportServices = "AirportServices";
        public const string PriorityServices = "PriorityServices";
        public const string Connectivity = "Connectivity";
        public const string Upgrades = "Upgrades";

        private static readonly IReadOnlyDictionary<AncillaryProfile, string> Routes = new Dictionary<AncillaryProfile, string>
        {
            [AncillaryProfile.Baggage] = Baggages,
            [AncillaryProfile.Meal] = Meals,
            [AncillaryProfile.Pet] = Pets,
            [AncillaryProfile.AssistedTravel] = AssistedTravel,
            [AncillaryProfile.AirportService] = AirportServices,
            [AncillaryProfile.Priority] = PriorityServices,
            [AncillaryProfile.Connectivity] = Connectivity,
            [AncillaryProfile.Upgrade] = Upgrades
        };

        public static string Of(AncillaryProfile profile)
            => Routes.TryGetValue(profile, out var route) ? route : throw new ArgumentOutOfRangeException(nameof(profile), profile, null);

        public static AncillaryProfile? ProfileOf(string? family)
            => Routes.Where(route => string.Equals(route.Value, family, StringComparison.OrdinalIgnoreCase)).Select(route => (AncillaryProfile?)route.Key).FirstOrDefault();
    }
}
