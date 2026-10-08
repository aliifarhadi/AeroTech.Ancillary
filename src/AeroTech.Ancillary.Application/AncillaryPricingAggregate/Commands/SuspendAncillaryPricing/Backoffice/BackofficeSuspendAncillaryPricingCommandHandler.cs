using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing.Backoffice
{
    public sealed class BackofficeSuspendAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeSuspendAncillaryPricingCommand, PricingResult>
    {
        private readonly ISuspendAncillaryPricingService _service;

        public BackofficeSuspendAncillaryPricingCommandHandler(ISuspendAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeSuspendAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
