using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode.Backoffice
{
    public sealed record BackofficeReactivateServiceSubCodeCommand(
        long ServiceSubCodeId) : IRequest<ServiceSubCodeResult>, IReactivateServiceSubCodeCommand;
}
