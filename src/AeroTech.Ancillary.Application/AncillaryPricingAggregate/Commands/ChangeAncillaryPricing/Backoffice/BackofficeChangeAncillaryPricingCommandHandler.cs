using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice
{
    public sealed class BackofficeChangeAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeChangeAncillaryPricingCommand, PricingResult>
    {
        private readonly IChangeAncillaryPricingService _service;

        public BackofficeChangeAncillaryPricingCommandHandler(IChangeAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeChangeAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
