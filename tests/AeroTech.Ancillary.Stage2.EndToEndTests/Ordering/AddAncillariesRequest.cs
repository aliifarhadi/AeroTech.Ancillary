namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed record AddAncillariesRequest(int? ExpectedOrderCommercialVersion, IReadOnlyList<string>? PricedSelectionRefs, string? IdempotencyKey);
