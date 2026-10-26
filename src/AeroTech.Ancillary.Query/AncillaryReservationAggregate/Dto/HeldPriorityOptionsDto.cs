namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldPriorityOptionsDto(HeldPrioritySelectionDto Priority);

    public sealed record HeldPrioritySelectionDto(bool OptIn);
}
