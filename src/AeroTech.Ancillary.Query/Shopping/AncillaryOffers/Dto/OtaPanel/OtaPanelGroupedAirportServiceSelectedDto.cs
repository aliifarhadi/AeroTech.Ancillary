namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedAirportServiceSelectedDto(
        int? AirportId,
        long? FacilityRef,
        DateTimeOffset? TimeWithOffset,
        int? GuestCount) : OtaPanelGroupedSelectedDto;
}
