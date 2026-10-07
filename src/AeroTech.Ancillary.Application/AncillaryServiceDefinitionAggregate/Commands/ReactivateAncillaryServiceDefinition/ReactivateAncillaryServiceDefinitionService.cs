using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition
{
    public sealed class ReactivateAncillaryServiceDefinitionService : IReactivateAncillaryServiceDefinitionService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ReactivateAncillaryServiceDefinitionService(
            IAncillaryServiceDefinitionRepository definitions,
            ISupplierRepository suppliers,
            IAncillaryServiceDefinitionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _definitions = definitions;
            _suppliers = suppliers;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceDefinitionResult> ReactivateAsync(IReactivateAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            var supplier = await _suppliers.GetAsync(definition.SupplierId, cancellationToken)
                           ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

            definition.Reactivate(supplier);

            if (await _definitions.HasActiveAsync(definition.OwnerAirlineId, definition.ServiceDefinitionRef, cancellationToken))
                throw ExceptionFactory.ServiceDefinitionRefAlreadyActive(definition.ServiceDefinitionRef);

            await _synchronizer.ProjectAsync(definition.ToReadModelSnapshot(supplier.Name), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
