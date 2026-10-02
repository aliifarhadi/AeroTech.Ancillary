using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule
{
    public sealed class ActivateAncillaryPriceRuleService : IActivateAncillaryPriceRuleService
    {
        private readonly IAncillaryPriceRuleRepository _rules;
        private readonly IAncillaryPriceRuleQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateAncillaryPriceRuleService(
            IAncillaryPriceRuleRepository rules,
            IAncillaryPriceRuleQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _rules = rules;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<AncillaryPriceRuleResult> ActivateAsync(IActivateAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default)
        {
            var rule = await _rules.GetAsync(command.AncillaryPriceRuleId, cancellationToken)
                       ?? throw ExceptionFactory.AncillaryPriceRuleNotFound();

            rule.Activate();

            if (await _rules.ActivePriorityExistsAsync(rule.OwnerAirlineId, rule.ProductRef, rule.CurrencyId, rule.Priority, rule.Id, cancellationToken))
                throw ExceptionFactory.AncillaryPriceRulePriorityAlreadyTaken();

            await _synchronizer.ProjectAsync(rule.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryPriceRuleResult(rule.Id, rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId, rule.Status);
        }
    }
}
