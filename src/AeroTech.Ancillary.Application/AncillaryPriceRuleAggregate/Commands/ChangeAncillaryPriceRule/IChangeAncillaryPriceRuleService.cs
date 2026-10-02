using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule
{
    public interface IChangeAncillaryPriceRuleService
    {
        Task<AncillaryPriceRuleResult> ChangeAsync(IChangeAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
