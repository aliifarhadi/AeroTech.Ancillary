using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public sealed class DefineAncillaryServiceDefinitionService : IDefineAncillaryServiceDefinitionService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryServiceDefinitionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineAncillaryServiceDefinitionService(
            IAncillaryServiceDefinitionRepository definitions,
            ISupplierRepository suppliers,
            IAncillaryServiceDefinitionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _definitions = definitions;
            _suppliers = suppliers;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<ServiceDefinitionResult> DefineAsync(
            IDefineAncillaryServiceDefinitionCommand command,
            CancellationToken cancellationToken = default)
        {
            var supplier = await _suppliers.GetAsync(command.SupplierId, cancellationToken)
                           ?? throw ExceptionFactory.ServiceDefinitionSupplierNotFound();

            var version = await _definitions.MaxVersionAsync(command.OwnerAirlineId, command.ServiceDefinitionRef, cancellationToken) + 1;

            var definition = AncillaryServiceDefinition.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                supplier.Id,
                command.ServiceDefinitionRef,
                version,
                command.ServiceSubCode,
                command.SubCodeSource,
                new ServiceDefinitionClassificationArgs(
                    command.ServiceTypeCode,
                    command.GroupCode,
                    command.SubGroupCode,
                    command.Description1Code,
                    command.Description2Code),
                command.CommercialName,
                command.Description,
                DocumentDefinition.Create(command.Document.Type, command.Document.Rfic, command.Document.Rfisc),
                BookingDefinition.Create(command.Booking.Method, command.Booking.SsrCode, command.Booking.SsimCode),
                command.SalesEffectiveFrom,
                command.SalesDiscontinueOn,
                _clock.GetDateTime());

            await _definitions.AddAsync(definition, cancellationToken);
            await _synchronizer.ProjectAsync(definition.ToReadModelSnapshot(supplier.Name), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return definition.ToResult();
        }
    }
}
