namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed record OrderMutationRequest(string Kind, string? TravellerRef = null, string? BoundId = null, bool? OpenForUse = null);
