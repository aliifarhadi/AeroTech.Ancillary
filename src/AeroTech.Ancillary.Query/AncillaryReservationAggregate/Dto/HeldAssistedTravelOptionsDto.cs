namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldAssistedTravelOptionsDto(HeldAssistedTravelSelectionDto AssistedTravel);

    public sealed record HeldAssistedTravelSelectionDto(string AssistanceSsrCode);
}
