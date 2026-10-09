using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
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
                    command.AirportServiceRule),
                _idGenerator,
                _clock.GetDateTime());

            await _provisions.AddAsync(provision, cancellationToken);
            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provision.ToResult();
        }
    }
}
