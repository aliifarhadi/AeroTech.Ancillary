using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing.Backoffice
{
    public sealed class BackofficeReviseAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeReviseAncillaryPricingCommand, PricingResult>
    {
        private readonly IReviseAncillaryPricingService _service;

        public BackofficeReviseAncillaryPricingCommandHandler(IReviseAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeReviseAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.ReviseAsync(command, cancellationToken);
    }
}
