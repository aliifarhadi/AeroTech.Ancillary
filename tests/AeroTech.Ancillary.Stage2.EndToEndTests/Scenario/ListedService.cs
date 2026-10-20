using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed record ListedService(string Surface, string OfferItemRef, string VariantCode, IReadOnlyList<string> Travellers, bool CanPurchase, JsonObject Node);
