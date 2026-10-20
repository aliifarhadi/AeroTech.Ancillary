namespace AeroTech.Ancillary.Application.Orders
{
    public sealed record OrderTravellerFacts(string TravellerRef, long OrderTravellerId, DateOnly? DateOfBirth, string? InfantOfTravellerRef);
}
