namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed record BatchLine(string ClientSelectionId, string Status, IReadOnlyList<string> ReasonCodes, bool CanPurchase, string? PricedSelectionRef);
