using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule
{
    public interface IRetireAncillaryPriceRuleService
    {
        Task<AncillaryPriceRuleResult> RetireAsync(IRetireAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
