using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing
{
    public sealed class ReactivateAncillaryPricingService : IReactivateAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ICurrencyReference _currencies;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ReactivateAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            ICurrencyReference currencies,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _currencies = currencies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<PricingResult> ReactivateAsync(IReactivateAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var provision = await _provisions.GetAsync(pricing.AncillaryProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.PricingProvisionNotFound();

            pricing.Reactivate(await _currencies.FindDecimalPlacesAsync(pricing, cancellationToken));

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            provision.EnsurePriceable();
            pricing.EnsureSameUnit(definition);

            if (await _pricings.FindActiveAsync(provision.Id, cancellationToken) is not null)
                throw ExceptionFactory.PricingAlreadyActive();

            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
