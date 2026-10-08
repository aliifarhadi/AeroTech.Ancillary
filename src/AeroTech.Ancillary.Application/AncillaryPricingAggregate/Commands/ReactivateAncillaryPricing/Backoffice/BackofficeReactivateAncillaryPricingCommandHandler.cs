using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing.Backoffice
{
    public sealed class BackofficeReactivateAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeReactivateAncillaryPricingCommand, PricingResult>
    {
        private readonly IReactivateAncillaryPricingService _service;

        public BackofficeReactivateAncillaryPricingCommandHandler(IReactivateAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeReactivateAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.ReactivateAsync(command, cancellationToken);
    }
}
