using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition
{
    public sealed class ChangeAncillaryServiceDefinitionService : IChangeAncillaryServiceDefinitionService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeAncillaryServiceDefinitionService(
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

        public async Task<ServiceDefinitionResult> ChangeAsync(
            IChangeAncillaryServiceDefinitionCommand command,
            CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(command.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            var supplier = await _suppliers.GetAsync(command.SupplierId, cancellationToken)
                           ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

            await _definitions.EnsurePricingUnitAllowedAsync(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                command.PricingUnit,
                cancellationToken);
            await _definitions.EnsureServiceDateBasisAllowedAsync(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                command.ServiceDateBasis,
                cancellationToken);

            await _definitions.EnsureProfileAllowedAsync(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                command.Profile,
                command.VariantCode,
                cancellationToken);

            definition.Change(
                supplier.Id,
                command.ServiceSubCode,
                command.SubCodeSource,
                new ServiceDefinitionClassificationArgs(
                    command.ServiceTypeCode,
                    command.GroupCode,
                    command.SubGroupCode,
                    command.Description1Code,
                    command.Description2Code),
                new ServiceDefinitionProfileArgs(
                    command.Profile,
                    command.VariantCode,
                    command.DocumentRouting,
                    ServiceSpecificationInputMapper.ToArgs(command.Specification ?? new ServiceSpecificationInput())),
                command.PricingUnit,
                command.ServiceDateBasis,
                command.CommercialName,
                command.Description,
                DocumentDefinition.Create(command.Document.Type, command.Document.Rfic, command.Document.Rfisc),
                BookingDefinition.Create(command.Booking.Method, command.Booking.SsrCode, command.Booking.SsimCode, command.Booking.ConfirmationRequirement ?? ConfirmationRequirement.Immediate),
                command.SalesEffectiveFrom,
                command.SalesDiscontinueOn);

            await _synchronizer.ProjectAsync(definition.ToReadModelSnapshot(supplier.Name), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
