using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldBaggageOptionsDto(HeldBaggageSelectionDto Baggage);

    public sealed record HeldBaggageSelectionDto(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? BagRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MeasuredWeightKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] OfferDimensionsDto? Dimensions,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightKg);
}
