using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record CanonicalAncillarySelectionEvaluation(
        SelectionEvaluationStatus Status,
        CanonicalAncillaryOfferCandidate Candidate,
        bool ProvisionChangedBySelection,
        IReadOnlyList<SelectionFieldIssue> FieldIssues);

    public sealed record SelectionFieldIssue(string Field, string ReasonCode);
}
