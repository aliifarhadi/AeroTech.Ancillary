using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct
{
    public sealed class ChangeAncillaryProductService : IChangeAncillaryProductService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IServiceSubCodeRepository _subCodes;
        private readonly IAncillaryProductQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeAncillaryProductService(
            IAncillaryProductRepository products,
            IServiceSubCodeRepository subCodes,
            IAncillaryProductQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _products = products;
            _subCodes = subCodes;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<AncillaryProductResult> ChangeAsync(IChangeAncillaryProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await _products.GetAsync(command.AncillaryProductId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryProductNotFound();

            SoldCombinations.EnsureSold(product.Type, command.SalesScope, command.Document.Type, command.InventoryControl, command.Quantity.Unit);

            var quantity = new QuantityPolicy(command.Quantity.Unit, command.Quantity.Min, command.Quantity.Max);
            var terms = new SalesTerms(
                command.Terms.Refundable,
                command.Terms.Commissionable,
                command.Terms.Reusable,
                command.Terms.FormOfRefundCode,
                command.Terms.InterlineSettlementAllowed);
            var baggage = command.Baggage is null
                ? null
                : new BaggageDetail(command.Baggage.Pieces, command.Baggage.Weight, command.Baggage.WeightUnit);
            var lounge = command.Lounge is null
                ? null
                : new LoungeDetail(command.Lounge.AirportIds);
            var subCode = command.Document.Rfisc is null
                ? null
                : await _subCodes.FindActiveAsync(product.OwnerAirlineId, command.Document.Rfisc, cancellationToken);

            product.Change(
                command.Name,
                command.Description,
                command.SalesScope,
                quantity,
                command.Document.Type,
                command.Document.Rfisc,
                command.Codes.ServiceTypeCode,
                terms,
                command.InventoryControl,
                baggage,
                lounge,
                subCode);

            await _synchronizer.ProjectAsync(product.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryProductResult(product.Id, product.OwnerAirlineId, product.ProductRef, product.Version, product.Status);
        }
    }
}
