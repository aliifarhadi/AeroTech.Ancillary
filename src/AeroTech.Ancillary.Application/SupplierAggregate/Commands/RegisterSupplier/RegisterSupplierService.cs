using AeroTech.Ancillary.Application.SupplierAggregate.Projection;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    public sealed class RegisterSupplierService : IRegisterSupplierService
    {
        private readonly ISupplierRepository _suppliers;
        private readonly ISupplierQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public RegisterSupplierService(
            ISupplierRepository suppliers,
            ISupplierQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _suppliers = suppliers;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<SupplierResult> RegisterAsync(IRegisterSupplierCommand command, CancellationToken cancellationToken = default)
        {
            var supplier = Supplier.Register(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.Name,
                command.FulfillmentKind,
                command.FulfillmentProviderKey,
                _clock.GetDateTime());

            await _suppliers.AddAsync(supplier, cancellationToken);
            await _synchronizer.ProjectAsync(supplier.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return supplier.ToResult();
        }
    }
}
