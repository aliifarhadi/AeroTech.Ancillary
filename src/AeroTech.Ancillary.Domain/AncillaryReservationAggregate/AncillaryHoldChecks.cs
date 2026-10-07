using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate
{
    public static class AncillaryHoldChecks
    {
        public static void Accept(
            AncillaryReservationUnitArgs unit,
            AncillaryServiceDefinition? definition,
            AncillaryProvision? provision,
            Supplier? supplier)
        {
            if (definition is null)
                throw ExceptionFactory.AncillaryHoldServiceDefinitionNotFound();

            if (provision is null || provision.ServiceDefinitionId != definition.Id)
                throw ExceptionFactory.AncillaryHoldProvisionMismatch();

            if (supplier is null || supplier.Id != definition.SupplierId)
                throw ExceptionFactory.SupplierNotFound();

            if (supplier.FulfillmentKind == SupplierFulfillmentKind.External)
                throw ExceptionFactory.SupplierFulfillmentProviderNotRegistered(supplier.FulfillmentProviderKey);

            if (unit.Quantity < provision.Quantity.MinQuantity || unit.Quantity > provision.Quantity.MaxQuantity)
                throw ExceptionFactory.AncillaryHoldQuantityNotAllowed();

            if (unit.CoverageScope != provision.CoverageScope)
                throw ExceptionFactory.AncillaryHoldCoverageMismatch();
        }
    }
}
