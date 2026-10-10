namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record CanonicalAncillaryOfferResult(
        int ContextSchemaVersion,
        DateTimeOffset EvaluatedAtUtc,
        int OwnerAirlineId,
        long PointOfSaleId,
        int RequestedCurrencyId,
        string? SourceVersion,
        IReadOnlyList<CanonicalAncillaryOfferCandidate> Candidates,
        IReadOnlyList<ShoppingDiagnostic> Diagnostics,
        bool HasBlockedCandidates);
}
