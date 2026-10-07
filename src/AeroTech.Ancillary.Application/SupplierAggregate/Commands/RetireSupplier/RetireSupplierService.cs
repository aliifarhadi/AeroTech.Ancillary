using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Ancillary.Application.SupplierAggregate.Projection;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier
{
    public sealed class RetireSupplierService : IRetireSupplierService
    {
        private readonly ISupplierRepository _suppliers;
        private readonly ISupplierQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public RetireSupplierService(
            ISupplierRepository suppliers,
            ISupplierQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _suppliers = suppliers;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<SupplierResult> RetireAsync(IRetireSupplierCommand command, CancellationToken cancellationToken = default)
        {
            var supplier = await _suppliers.GetAsync(command.SupplierId, cancellationToken)
                           ?? throw ExceptionFactory.SupplierNotFound();

            supplier.Retire(_clock.GetDateTime());

            await _synchronizer.ProjectAsync(supplier.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return supplier.ToResult();
        }
    }
}
