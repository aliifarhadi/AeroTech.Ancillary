using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed class DefineAncillaryPricingService : IDefineAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ICurrencyReference _currencies;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            ICurrencyReference currencies,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _currencies = currencies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<PricingResult> DefineAsync(IDefineAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.AncillaryProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.PricingProvisionNotFound();

            provision.EnsurePriceable();

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();
            var pricingUnit = definition.PricingUnit ?? throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();
            var version = await _pricings.MaxVersionAsync(provision.Id, cancellationToken) + 1;
            var rates = command.Rates.ToArgs();
            var pricing = AncillaryPricing.Define(
                _idGenerator.NewId(),
                provision.Id,
                pricingUnit,
                version,
                rates,
                await _currencies.FindDecimalPlacesAsync(rates, cancellationToken),
                _idGenerator,
                _clock.GetDateTime());

            await _pricings.AddAsync(pricing, cancellationToken);
            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
