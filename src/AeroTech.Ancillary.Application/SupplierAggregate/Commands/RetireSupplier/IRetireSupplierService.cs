using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier
{
    public interface IRetireSupplierService
    {
        Task<SupplierResult> RetireAsync(IRetireSupplierCommand command, CancellationToken cancellationToken = default);
    }
}
