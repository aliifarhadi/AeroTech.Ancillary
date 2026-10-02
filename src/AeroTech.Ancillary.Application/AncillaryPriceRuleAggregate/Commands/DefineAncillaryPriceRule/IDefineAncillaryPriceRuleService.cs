namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public interface IDefineAncillaryPriceRuleService
    {
        Task<AncillaryPriceRuleResult> DefineAsync(IDefineAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
