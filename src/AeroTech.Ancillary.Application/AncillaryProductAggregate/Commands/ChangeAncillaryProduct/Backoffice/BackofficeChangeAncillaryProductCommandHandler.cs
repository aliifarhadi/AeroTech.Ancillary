using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice
{
    public sealed class BackofficeChangeAncillaryProductCommandHandler : IRequestHandler<BackofficeChangeAncillaryProductCommand, AncillaryProductResult>
    {
        private readonly IChangeAncillaryProductService _service;

        public BackofficeChangeAncillaryProductCommandHandler(IChangeAncillaryProductService service) => _service = service;

        public Task<AncillaryProductResult> Handle(BackofficeChangeAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
