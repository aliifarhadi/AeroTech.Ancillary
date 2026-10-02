using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode.Backoffice
{
    public sealed class BackofficeRetireServiceSubCodeCommandHandler : IRequestHandler<BackofficeRetireServiceSubCodeCommand, ServiceSubCodeResult>
    {
        private readonly IRetireServiceSubCodeService _service;

        public BackofficeRetireServiceSubCodeCommandHandler(IRetireServiceSubCodeService service) => _service = service;

        public Task<ServiceSubCodeResult> Handle(BackofficeRetireServiceSubCodeCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
