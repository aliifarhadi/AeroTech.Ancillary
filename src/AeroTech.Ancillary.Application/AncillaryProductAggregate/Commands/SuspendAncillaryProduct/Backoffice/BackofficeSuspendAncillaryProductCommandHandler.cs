using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct.Backoffice
{
    public sealed class BackofficeSuspendAncillaryProductCommandHandler : IRequestHandler<BackofficeSuspendAncillaryProductCommand, AncillaryProductResult>
    {
        private readonly ISuspendAncillaryProductService _service;

        public BackofficeSuspendAncillaryProductCommandHandler(ISuspendAncillaryProductService service) => _service = service;

        public Task<AncillaryProductResult> Handle(BackofficeSuspendAncillaryProductCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
