using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode.Backoffice
{
    public sealed class BackofficeReactivateServiceSubCodeCommandHandler : IRequestHandler<BackofficeReactivateServiceSubCodeCommand, ServiceSubCodeResult>
    {
        private readonly IReactivateServiceSubCodeService _service;

        public BackofficeReactivateServiceSubCodeCommandHandler(IReactivateServiceSubCodeService service) => _service = service;

        public Task<ServiceSubCodeResult> Handle(BackofficeReactivateServiceSubCodeCommand command, CancellationToken cancellationToken)
            => _service.ReactivateAsync(command, cancellationToken);
    }
}
