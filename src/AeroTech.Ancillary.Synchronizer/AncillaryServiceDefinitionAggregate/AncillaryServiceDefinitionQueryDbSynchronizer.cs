using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryServiceDefinitionAggregate
{
    public sealed class AncillaryServiceDefinitionQueryDbSynchronizer : IAncillaryServiceDefinitionQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AncillaryServiceDefinitionQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AncillaryServiceDefinitionReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var definition = await _dbContext.AncillaryServiceDefinitions
                .FirstOrDefaultAsync(row => row.Id == snapshot.ServiceDefinitionId, cancellationToken);

            if (definition is null)
            {
                definition = new AncillaryServiceDefinitionReadModel { Id = snapshot.ServiceDefinitionId };
                _dbContext.AncillaryServiceDefinitions.Add(definition);
            }

            definition.OwnerAirlineId = snapshot.OwnerAirlineId;
            definition.SupplierId = snapshot.SupplierId;
            definition.SupplierName = snapshot.SupplierName;
            definition.ServiceDefinitionRef = snapshot.ServiceDefinitionRef;
            definition.Version = snapshot.Version;
            definition.ServiceTypeCode = snapshot.ServiceTypeCode;
            definition.ServiceSubCode = snapshot.ServiceSubCode;
            definition.SubCodeSource = snapshot.SubCodeSource;
            definition.GroupCode = snapshot.GroupCode;
            definition.SubGroupCode = snapshot.SubGroupCode;
            definition.Description1Code = snapshot.Description1Code;
            definition.Description2Code = snapshot.Description2Code;
            definition.CommercialName = snapshot.CommercialName;
            definition.Description = snapshot.Description;
            definition.DocumentType = snapshot.DocumentType;
            definition.DocumentRfic = snapshot.DocumentRfic;
            definition.DocumentRfisc = snapshot.DocumentRfisc;
            definition.BookingMethod = snapshot.BookingMethod;
            definition.BookingSsrCode = snapshot.BookingSsrCode;
            definition.BookingSsimCode = snapshot.BookingSsimCode;
            definition.SalesEffectiveFrom = snapshot.SalesEffectiveFrom;
            definition.SalesDiscontinueOn = snapshot.SalesDiscontinueOn;
            definition.Status = snapshot.Status;
            definition.CreatedAt = snapshot.CreatedAt;
            definition.ActivatedAt = snapshot.ActivatedAt;
            definition.SuspendedAt = snapshot.SuspendedAt;
            definition.RetiredAt = snapshot.RetiredAt;
            definition.LastUpdateTime = _clock.GetDateTime();
        }
    }
}
