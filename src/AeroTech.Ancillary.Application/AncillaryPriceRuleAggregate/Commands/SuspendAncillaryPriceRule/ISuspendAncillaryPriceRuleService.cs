using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule
{
    public interface ISuspendAncillaryPriceRuleService
    {
        Task<AncillaryPriceRuleResult> SuspendAsync(ISuspendAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
