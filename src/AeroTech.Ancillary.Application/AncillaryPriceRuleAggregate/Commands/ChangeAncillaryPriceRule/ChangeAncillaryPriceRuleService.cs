using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule
{
    public sealed class ChangeAncillaryPriceRuleService : IChangeAncillaryPriceRuleService
    {
        private readonly IAncillaryPriceRuleRepository _rules;
        private readonly IAncillaryPriceRuleQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;

        public ChangeAncillaryPriceRuleService(
            IAncillaryPriceRuleRepository rules,
            IAncillaryPriceRuleQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator)
        {
            _rules = rules;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
        }

        public async Task<AncillaryPriceRuleResult> ChangeAsync(IChangeAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default)
        {
            var rule = await _rules.GetAsync(command.AncillaryPriceRuleId, cancellationToken)
                       ?? throw ExceptionFactory.AncillaryPriceRuleNotFound();

            var lines = command.Lines
                .Select(line => new PriceLineArgs(line.Category, line.Code, line.Name, line.Amount))
                .ToList();
            var conditions = new PriceRuleConditions(
                command.Conditions.PassengerTypes,
                command.Conditions.OriginAirportIds,
                command.Conditions.DestinationAirportIds);

            rule.Change(
                command.Priority,
                command.CurrencyId,
                lines,
                command.SalesFrom,
                command.SalesTo,
                command.TravelFrom,
                command.TravelTo,
                conditions,
                _idGenerator);

            await _synchronizer.ProjectAsync(rule.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryPriceRuleResult(rule.Id, rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId, rule.Status);
        }
    }
}
