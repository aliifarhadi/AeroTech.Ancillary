using MediatR;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice
{
    public sealed record BackofficeRegisterServiceSubCodeCommand(
        int OwnerAirlineId,
        string Code,
        string? Rfic,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string? CommercialName) : IRequest<ServiceSubCodeResult>, IRegisterServiceSubCodeCommand;
}
