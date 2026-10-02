using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule
{
    public sealed class SuspendAncillaryPriceRuleService : ISuspendAncillaryPriceRuleService
    {
        private readonly IAncillaryPriceRuleRepository _rules;
        private readonly IAncillaryPriceRuleQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public SuspendAncillaryPriceRuleService(
            IAncillaryPriceRuleRepository rules,
            IAncillaryPriceRuleQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _rules = rules;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<AncillaryPriceRuleResult> SuspendAsync(ISuspendAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default)
        {
            var rule = await _rules.GetAsync(command.AncillaryPriceRuleId, cancellationToken)
                       ?? throw ExceptionFactory.AncillaryPriceRuleNotFound();

            rule.Suspend();

            await _synchronizer.ProjectAsync(rule.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryPriceRuleResult(rule.Id, rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId, rule.Status);
        }
    }
}
