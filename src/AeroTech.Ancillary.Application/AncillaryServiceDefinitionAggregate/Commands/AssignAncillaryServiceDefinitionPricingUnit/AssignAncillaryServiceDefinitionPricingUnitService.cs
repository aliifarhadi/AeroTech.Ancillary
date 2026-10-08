using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit
{
    public sealed class AssignAncillaryServiceDefinitionPricingUnitService : IAssignAncillaryServiceDefinitionPricingUnitService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _definitionSynchronizer;
        private readonly IAncillaryPricingQueryDbSynchronizer _pricingSynchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public AssignAncillaryServiceDefinitionPricingUnitService(
            IAncillaryServiceDefinitionRepository definitions,
            ISupplierRepository suppliers,
            IAncillaryPricingRepository pricings,
            IAncillaryServiceDefinitionQueryDbSynchronizer definitionSynchronizer,
            IAncillaryPricingQueryDbSynchronizer pricingSynchronizer,
            IUnitOfWork unitOfWork)
        {
            _definitions = definitions;
            _suppliers = suppliers;
            _pricings = pricings;
            _definitionSynchronizer = definitionSynchronizer;
            _pricingSynchronizer = pricingSynchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceDefinitionResult> AssignAsync(IAssignAncillaryServiceDefinitionPricingUnitCommand command, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();
            var versions = await _definitions.ListVersionsAsync(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                cancellationToken);

            if (versions.Any(version => version.PricingUnit is not null && version.PricingUnit != command.PricingUnit))
                throw ExceptionFactory.ServiceDefinitionPricingUnitConflict(command.PricingUnit);

            var unassigned = versions.Where(version => version.PricingUnit is null).ToList();

            if (unassigned.Count == 0)
                throw ExceptionFactory.ServiceDefinitionPricingUnitAlreadyAssigned();

            foreach (var version in unassigned)
            {
                var supplier = await _suppliers.GetAsync(version.SupplierId, cancellationToken)
                               ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

                version.AssignPricingUnit(command.PricingUnit);
                await _definitionSynchronizer.ProjectAsync(version.ToReadModelSnapshot(supplier.Name), cancellationToken);
            }

            var pricings = await _pricings.ListUnassignedAsync(
                unassigned.Select(version => version.Id).ToList(),
                cancellationToken);

            foreach (var pricing in pricings)
            {
                pricing.AssignPricingUnit(command.PricingUnit);
                await _pricingSynchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
