namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode
{
    public interface IRegisterServiceSubCodeService
    {
        Task<ServiceSubCodeResult> RegisterAsync(IRegisterServiceSubCodeCommand command, CancellationToken cancellationToken = default);
    }
}
