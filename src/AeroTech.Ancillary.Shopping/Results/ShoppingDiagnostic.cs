using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record ShoppingDiagnostic(
        string Code,
        ShoppingDiagnosticSeverity Severity,
        CandidateIdentity? CandidateIdentity,
        IReadOnlyList<string> MissingContextFields,
        string? SourceAuthority,
        ShoppingEvidenceKind EvidenceKind);
}
