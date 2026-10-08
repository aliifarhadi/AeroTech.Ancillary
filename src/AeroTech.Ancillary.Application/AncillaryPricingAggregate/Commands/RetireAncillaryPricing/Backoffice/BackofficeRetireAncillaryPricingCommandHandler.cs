using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing.Backoffice
{
    public sealed class BackofficeRetireAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeRetireAncillaryPricingCommand, PricingResult>
    {
        private readonly IRetireAncillaryPricingService _service;

        public BackofficeRetireAncillaryPricingCommandHandler(IRetireAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeRetireAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
