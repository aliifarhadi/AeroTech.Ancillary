using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct.Backoffice
{
    public sealed class BackofficeReviseAncillaryProductCommandHandler : IRequestHandler<BackofficeReviseAncillaryProductCommand, AncillaryProductResult>
    {
        private readonly IReviseAncillaryProductService _service;

        public BackofficeReviseAncillaryProductCommandHandler(IReviseAncillaryProductService service) => _service = service;

        public Task<AncillaryProductResult> Handle(BackofficeReviseAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.ReviseAsync(command, cancellationToken);
    }
}
