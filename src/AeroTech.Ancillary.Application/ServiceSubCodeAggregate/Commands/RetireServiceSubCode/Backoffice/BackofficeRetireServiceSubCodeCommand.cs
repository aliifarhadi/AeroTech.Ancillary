using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode.Backoffice
{
    public sealed record BackofficeRetireServiceSubCodeCommand(
        long ServiceSubCodeId) : IRequest<ServiceSubCodeResult>, IRetireServiceSubCodeCommand;
}
