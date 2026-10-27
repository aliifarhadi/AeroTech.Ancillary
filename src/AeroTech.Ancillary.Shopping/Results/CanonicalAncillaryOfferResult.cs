using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
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
        bool HasBlockedCandidates,
        IReadOnlyList<PublishedRateOfExchange> RatesOfExchange,
        IReadOnlyList<AncillaryServiceDefinition> Definitions,
        IReadOnlyList<AncillaryProvision> Provisions);
}
