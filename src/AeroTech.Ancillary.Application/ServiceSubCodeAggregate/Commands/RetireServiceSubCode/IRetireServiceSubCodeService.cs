using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode
{
    public interface IRetireServiceSubCodeService
    {
        Task<ServiceSubCodeResult> RetireAsync(IRetireServiceSubCodeCommand command, CancellationToken cancellationToken = default);
    }
}
