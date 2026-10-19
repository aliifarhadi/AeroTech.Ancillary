namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelTravellerGroupDto(
        string TravellerRef,
        string PassengerTypeCode,
        IReadOnlyList<OtaPanelFlightGroupDto> FlightGroups);
}
