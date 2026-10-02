using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice
{
    public sealed class BackofficeRegisterServiceSubCodeCommandHandler : IRequestHandler<BackofficeRegisterServiceSubCodeCommand, ServiceSubCodeResult>
    {
        private readonly IRegisterServiceSubCodeService _service;

        public BackofficeRegisterServiceSubCodeCommandHandler(IRegisterServiceSubCodeService service) => _service = service;

        public Task<ServiceSubCodeResult> Handle(BackofficeRegisterServiceSubCodeCommand command, CancellationToken cancellationToken)
            => _service.RegisterAsync(command, cancellationToken);
    }
}
