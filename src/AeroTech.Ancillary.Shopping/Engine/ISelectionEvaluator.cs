using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Shopping.Engine
{
    public interface ISelectionEvaluator
    {
        Task<CanonicalAncillarySelectionEvaluation> EvaluateAsync(
            CandidateIdentity candidate,
            AncillarySelection selection,
            CancellationToken cancellationToken = default);
    }
}
