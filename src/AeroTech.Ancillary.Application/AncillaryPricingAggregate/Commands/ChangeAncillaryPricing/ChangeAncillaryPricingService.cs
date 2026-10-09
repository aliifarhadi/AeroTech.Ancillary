using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing
{
    public sealed class ChangeAncillaryPricingService : IChangeAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly ICurrencyReference _currencies;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;

        public ChangeAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            ICurrencyReference currencies,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator)
        {
            _pricings = pricings;
            _currencies = currencies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
        }

        public async Task<PricingResult> ChangeAsync(IChangeAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var rates = command.Rates.ToArgs();

            pricing.Change(rates, await _currencies.FindDecimalPlacesAsync(rates, cancellationToken), _idGenerator);

            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
