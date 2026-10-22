using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    [JsonDerivedType(typeof(BackofficeGroupedBaggageSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedMealSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedPetSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedAssistedTravelSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedAirportServiceSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedPrioritySelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedConnectivitySelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedUpgradeSelectedDto))]
    [JsonDerivedType(typeof(BackofficeGroupedExtraSeatSelectedDto))]
    public abstract record BackofficeGroupedSelectedDto;
}
