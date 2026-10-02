using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice
{
    public sealed class BackofficeDefineAncillaryProductCommandHandler : IRequestHandler<BackofficeDefineAncillaryProductCommand, AncillaryProductResult>
    {
        private readonly IDefineAncillaryProductService _service;

        public BackofficeDefineAncillaryProductCommandHandler(IDefineAncillaryProductService service) => _service = service;

        public Task<AncillaryProductResult> Handle(BackofficeDefineAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
