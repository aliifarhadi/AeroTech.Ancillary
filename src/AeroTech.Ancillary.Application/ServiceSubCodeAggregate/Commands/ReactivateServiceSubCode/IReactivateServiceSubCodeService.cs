using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode
{
    public interface IReactivateServiceSubCodeService
    {
        Task<ServiceSubCodeResult> ReactivateAsync(IReactivateServiceSubCodeCommand command, CancellationToken cancellationToken = default);
    }
}
