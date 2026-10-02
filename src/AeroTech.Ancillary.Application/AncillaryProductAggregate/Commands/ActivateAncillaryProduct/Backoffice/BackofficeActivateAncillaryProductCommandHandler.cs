using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct.Backoffice
{
    public sealed class BackofficeActivateAncillaryProductCommandHandler : IRequestHandler<BackofficeActivateAncillaryProductCommand, ActivateAncillaryProductResult>
    {
        private readonly IActivateAncillaryProductService _service;

        public BackofficeActivateAncillaryProductCommandHandler(IActivateAncillaryProductService service) => _service = service;

        public Task<ActivateAncillaryProductResult> Handle(BackofficeActivateAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
