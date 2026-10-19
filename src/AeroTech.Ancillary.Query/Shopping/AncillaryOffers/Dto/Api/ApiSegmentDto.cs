namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiSegmentDto(
        string Id,
        string? CarrierCode,
        string? Number,
        ApiEndpointDto Departure,
        ApiEndpointDto Arrival,
        ApiOperatingDto Operating);
}
