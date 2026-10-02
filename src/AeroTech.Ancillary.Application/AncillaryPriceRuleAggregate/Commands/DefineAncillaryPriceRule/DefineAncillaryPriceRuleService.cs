using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public sealed class DefineAncillaryPriceRuleService : IDefineAncillaryPriceRuleService
    {
        private readonly IAncillaryPriceRuleRepository _rules;
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryPriceRuleQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineAncillaryPriceRuleService(
            IAncillaryPriceRuleRepository rules,
            IAncillaryProductRepository products,
            IAncillaryPriceRuleQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _rules = rules;
            _products = products;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AncillaryPriceRuleResult> DefineAsync(IDefineAncillaryPriceRuleCommand command, CancellationToken cancellationToken = default)
        {
            if (!await _products.ExistsAsync(command.OwnerAirlineId, command.ProductRef, cancellationToken))
                throw ExceptionFactory.AncillaryPriceRuleProductDoesNotExist();

            var lines = command.Lines
                .Select(line => new PriceLineArgs(line.Category, line.Code, line.Name, line.Amount))
                .ToList();
            var conditions = new PriceRuleConditions(
                command.Conditions.PassengerTypes,
                command.Conditions.OriginAirportIds,
                command.Conditions.DestinationAirportIds);

            var rule = AncillaryPriceRule.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.ProductRef,
                command.Priority,
                command.CurrencyId,
                lines,
                command.SalesFrom,
                command.SalesTo,
                command.TravelFrom,
                command.TravelTo,
                conditions,
                _idGenerator,
                _clock.GetDateTime());

            await _rules.AddAsync(rule, cancellationToken);
            await _synchronizer.ProjectAsync(rule.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryPriceRuleResult(rule.Id, rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId, rule.Status);
        }
    }
}
