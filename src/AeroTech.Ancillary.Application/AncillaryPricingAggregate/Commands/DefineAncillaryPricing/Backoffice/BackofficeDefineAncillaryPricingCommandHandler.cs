using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice
{
    public sealed class BackofficeDefineAncillaryPricingCommandHandler
        : IRequestHandler<BackofficeDefineAncillaryPricingCommand, PricingResult>
    {
        private readonly IDefineAncillaryPricingService _service;

        public BackofficeDefineAncillaryPricingCommandHandler(IDefineAncillaryPricingService service) => _service = service;

        public Task<PricingResult> Handle(BackofficeDefineAncillaryPricingCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
