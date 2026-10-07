namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    public interface IRegisterSupplierService
    {
        Task<SupplierResult> RegisterAsync(IRegisterSupplierCommand command, CancellationToken cancellationToken = default);
    }
}
