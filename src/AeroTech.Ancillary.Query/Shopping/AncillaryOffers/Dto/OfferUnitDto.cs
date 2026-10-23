using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    [JsonDerivedType(typeof(OrderUnitDto))]
    [JsonDerivedType(typeof(TravellerUnitDto))]
    [JsonDerivedType(typeof(TravellerPortionUnitDto))]
    [JsonDerivedType(typeof(TravellerFlightUnitDto))]
    public abstract record OfferUnitDto;
}
