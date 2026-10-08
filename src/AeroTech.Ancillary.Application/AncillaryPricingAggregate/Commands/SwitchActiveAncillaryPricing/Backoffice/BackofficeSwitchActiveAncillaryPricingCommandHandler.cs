using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice
{
    public sealed class BackofficeSwitchActiveAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeSwitchActiveAncillaryPricingCommand, PricingResult>
    {
        private readonly ISwitchActiveAncillaryPricingService _service;

        public BackofficeSwitchActiveAncillaryPricingCommandHandler(ISwitchActiveAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeSwitchActiveAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.SwitchAsync(command, cancellationToken);
    }
}
