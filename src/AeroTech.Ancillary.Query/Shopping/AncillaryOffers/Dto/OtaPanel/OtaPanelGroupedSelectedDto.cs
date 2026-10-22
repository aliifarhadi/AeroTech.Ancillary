using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    [JsonDerivedType(typeof(OtaPanelGroupedBaggageSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedMealSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedPetSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedAssistedTravelSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedAirportServiceSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedPrioritySelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedConnectivitySelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedUpgradeSelectedDto))]
    [JsonDerivedType(typeof(OtaPanelGroupedExtraSeatSelectedDto))]
    public abstract record OtaPanelGroupedSelectedDto;
}
