using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryProductAggregate
{
    public sealed class AncillaryProductQueryDbSynchronizer : IAncillaryProductQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public AncillaryProductQueryDbSynchronizer(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task ProjectAsync(AncillaryProductReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var product = await _dbContext.AncillaryProducts.FirstOrDefaultAsync(row => row.Id == snapshot.AncillaryProductId, cancellationToken);

            if (product is null)
            {
                product = new AncillaryProductReadModel { Id = snapshot.AncillaryProductId };
                _dbContext.AncillaryProducts.Add(product);
            }

            product.OwnerAirlineId = snapshot.OwnerAirlineId;
            product.ProductRef = snapshot.ProductRef;
            product.Version = snapshot.Version;
            product.Type = snapshot.Type;
            product.Name = snapshot.Name;
            product.Description = snapshot.Description;
            product.SalesScope = snapshot.SalesScope;
            product.QuantityUnit = snapshot.QuantityUnit;
            product.QuantityMin = snapshot.QuantityMin;
            product.QuantityMax = snapshot.QuantityMax;
            product.DocumentType = snapshot.DocumentType;
            product.Rfisc = snapshot.Rfisc;
            product.Rfic = snapshot.Rfic;
            product.ServiceTypeCode = snapshot.ServiceTypeCode;
            product.GroupCode = snapshot.GroupCode;
            product.SubGroupCode = snapshot.SubGroupCode;
            product.Description1Code = snapshot.Description1Code;
            product.Description2Code = snapshot.Description2Code;
            product.Refundable = snapshot.Refundable;
            product.Commissionable = snapshot.Commissionable;
            product.Reusable = snapshot.Reusable;
            product.FormOfRefundCode = snapshot.FormOfRefundCode;
            product.InterlineSettlementAllowed = snapshot.InterlineSettlementAllowed;
            product.InventoryControl = snapshot.InventoryControl;
            product.BaggagePieces = snapshot.BaggagePieces;
            product.BaggageWeight = snapshot.BaggageWeight;
            product.BaggageWeightUnit = snapshot.BaggageWeightUnit;
            product.LoungeAirportIds = snapshot.LoungeAirportIds?.ToList();
            product.Status = snapshot.Status;
            product.CreatedAt = snapshot.CreatedAt;
            product.ActivatedAt = snapshot.ActivatedAt;
            product.RetiredAt = snapshot.RetiredAt;
        }
    }
}
