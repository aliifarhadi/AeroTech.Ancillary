using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition
{
    public sealed class RetireAncillaryServiceDefinitionService : IRetireAncillaryServiceDefinitionService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public RetireAncillaryServiceDefinitionService(
            IAncillaryServiceDefinitionRepository definitions,
            ISupplierRepository suppliers,
            IAncillaryServiceDefinitionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _definitions = definitions;
            _suppliers = suppliers;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ServiceDefinitionResult> RetireAsync(IRetireAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            var supplier = await _suppliers.GetAsync(definition.SupplierId, cancellationToken)
                           ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

            definition.Retire(_clock.GetDateTime());

            await _synchronizer.ProjectAsync(definition.ToReadModelSnapshot(supplier.Name), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
