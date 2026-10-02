using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule
{
    public interface IActivateAncillaryPriceRuleService
    {
        Task<AncillaryPriceRuleResult> ActivateAsync(IActivateAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
