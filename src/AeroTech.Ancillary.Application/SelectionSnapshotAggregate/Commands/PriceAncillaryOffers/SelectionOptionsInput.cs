using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SelectionOptionsInput(
        BaggageSelectionOptions? Baggage = null,
        PetSelectionOptions? Pet = null,
        AssistedTravelSelectionOptions? AssistedTravel = null,
        AirportServiceSelectionOptions? AirportService = null,
        PrioritySelectionOptions? Priority = null,
        ConnectivitySelectionOptions? Connectivity = null);
}
