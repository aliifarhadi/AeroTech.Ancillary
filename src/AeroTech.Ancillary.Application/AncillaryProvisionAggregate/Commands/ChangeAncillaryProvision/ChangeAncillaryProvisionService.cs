using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision
{
    public sealed class ChangeAncillaryProvisionService : IChangeAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly ICurrencyReference _currencies;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;

        public ChangeAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            ICurrencyReference currencies,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator)
        {
            _provisions = provisions;
            _definitions = definitions;
            _synchronizer = synchronizer;
            _currencies = currencies;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
        }

        public async Task<ProvisionResult> ChangeAsync(IChangeAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            if (command.ServiceDefinitionId is { } serviceDefinitionId && provision.ServiceDefinitionId != serviceDefinitionId)
                throw ExceptionFactory.ProvisionNotFound();

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            if (command.Profile is { } profile && definition.Profile != profile)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(definition.Profile, profile);

            provision.Change(
                definition,
                command.Sequence,
                command.CoverageScope,
                command.PurchaseStage,
                command.PriceOrigin,
                command.QuoteProviderKey,
                command.Quantity.ToRule(),
                command.ApplicationType,
                command.Outcome.ToOutcome(),
                command.Settlement.ToDefinition(),
                command.Availability.ToDefinition(),
                command.Fulfillment.ToDefinition(),
                ProvisionInputMapper.ToRules(
                    command.PassengerEligibility,
                    command.SalesRestrictions,
                    command.Geography,
                    command.FlightApplication,
                    command.FareApplication,
                    command.TravelDate,
                    command.DayTimeApplication,
                    command.AdvancePurchase,
                    command.BaggageApplication,
                    command.SeatApplication,
                    command.PetRule,
                    command.AssistedTravelRule,
                    command.AirportServiceRule,
                    command.Settlement.CancellationRules),
                await _currencies.FindDecimalPlacesAsync(command.Settlement.PenaltyCurrencyIds(), cancellationToken),
                _idGenerator);

            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provision.ToResult();
        }
    }
}
