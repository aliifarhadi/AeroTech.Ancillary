using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis
{
    public sealed class AssignAncillaryServiceDefinitionServiceDateBasisService : IAssignAncillaryServiceDefinitionServiceDateBasisService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public AssignAncillaryServiceDefinitionServiceDateBasisService(
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

        public async Task<ServiceDefinitionResult> AssignAsync(IAssignAncillaryServiceDefinitionServiceDateBasisCommand command, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();
            var versions = await _definitions.ListVersionsAsync(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                cancellationToken);

            if (versions.Any(version => version.ServiceDateBasis is not null && version.ServiceDateBasis != command.ServiceDateBasis))
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisConflict(command.ServiceDateBasis);

            var unassigned = versions.Where(version => version.ServiceDateBasis is null).ToList();

            if (unassigned.Count == 0)
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisAlreadyAssigned();

            foreach (var version in unassigned)
            {
                var supplier = await _suppliers.GetAsync(version.SupplierId, cancellationToken)
                               ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

                version.AssignServiceDateBasis(command.ServiceDateBasis);
                await _synchronizer.ProjectAsync(version.ToReadModelSnapshot(supplier.Name), cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
