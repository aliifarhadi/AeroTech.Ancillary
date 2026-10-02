using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct.Backoffice
{
    public sealed class BackofficeRetireAncillaryProductCommandHandler : IRequestHandler<BackofficeRetireAncillaryProductCommand, AncillaryProductResult>
    {
        private readonly IRetireAncillaryProductService _service;

        public BackofficeRetireAncillaryProductCommandHandler(IRetireAncillaryProductService service) => _service = service;

        public Task<AncillaryProductResult> Handle(BackofficeRetireAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
