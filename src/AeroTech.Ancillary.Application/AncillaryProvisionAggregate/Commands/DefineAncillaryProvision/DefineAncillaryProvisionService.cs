using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed class DefineAncillaryProvisionService : IDefineAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _provisions = provisions;
            _definitions = definitions;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<ProvisionResult> DefineAsync(IDefineAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            var provision = AncillaryProvision.Define(
                _idGenerator.NewId(),
                definition.Id,
                command.Sequence,
                command.SalesEffectiveFrom,
                command.SalesDiscontinueAt,
                command.CoverageScope,
                QuantityRule.Create(command.Quantity.Unit, command.Quantity.MinQuantity, command.Quantity.MaxQuantity),
                command.Application.Type,
                CommercialOutcome.Create(
                    command.Outcome.Disposition,
                    command.Outcome.DocumentRequired,
                    command.Outcome.BookingRequired),
                command.Fee is { } fee ? FeeDefinition.Create(fee.CurrencyId, fee.ApplicationUnit) : null,
                command.Fee?.PriceLines
                    .Select(line => new ProvisionPriceLineArgs(line.Category, line.Code, line.Name, line.UnitAmount))
                    .ToList() ?? [],
                SettlementDefinition.Create(
                    command.Settlement.ReissueRefund,
                    command.Settlement.FormOfRefund,
                    command.Settlement.Commissionable,
                    command.Settlement.InterlineSettlement),
                AvailabilityDefinition.Create(command.Availability.MustCheckAvailability),
                FulfillmentDefinition.Create(command.Fulfillment.FulfillmentProviderKey),
                _idGenerator,
                _clock.GetDateTime());

            await _provisions.AddAsync(provision, cancellationToken);
            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProvisionResult(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                provision.CoverageScope,
                provision.Outcome.Disposition,
                provision.Status);
        }
    }
}
