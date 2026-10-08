using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing.Backoffice
{
    public sealed class BackofficeActivateAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeActivateAncillaryPricingCommand, PricingResult>
    {
        private readonly IActivateAncillaryPricingService _service;

        public BackofficeActivateAncillaryPricingCommandHandler(IActivateAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeActivateAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
