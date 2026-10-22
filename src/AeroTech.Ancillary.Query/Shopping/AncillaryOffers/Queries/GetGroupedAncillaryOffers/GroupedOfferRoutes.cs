using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public static class GroupedOfferRoutes
    {
        public const string Backoffice = "Backoffice";
        public const string OtaPanel = "OtaPanel";

        public static string SeatMap(string surface) => $"/{surface}/v1/SeatMaps";

        public static string Details(string surface, AncillaryProfile profile) => $"/{surface}/v1/AncillaryOffers/{Family(profile)}/Details";

        public static string Family(AncillaryProfile profile)
            => profile switch
            {
                AncillaryProfile.Baggage => "Baggages",
                AncillaryProfile.Meal => "Meals",
                AncillaryProfile.Pet => "Pets",
                AncillaryProfile.AssistedTravel => "AssistedTravel",
                AncillaryProfile.AirportService => "AirportServices",
                AncillaryProfile.Priority => "PriorityServices",
                AncillaryProfile.Connectivity => "Connectivity",
                AncillaryProfile.Upgrade => "Upgrades",
                AncillaryProfile.Seat => "ExtraSeats",
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };
    }
}
