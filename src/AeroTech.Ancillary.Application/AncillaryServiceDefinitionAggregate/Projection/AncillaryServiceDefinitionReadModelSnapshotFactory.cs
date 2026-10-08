using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Projection
{
    internal static class AncillaryServiceDefinitionReadModelSnapshotFactory
    {
        public static AncillaryServiceDefinitionReadModelSnapshot ToReadModelSnapshot(
            this AncillaryServiceDefinition definition,
            string supplierName)
            => new(
                definition.Id,
                definition.OwnerAirlineId,
                definition.SupplierId,
                supplierName,
                definition.ServiceDefinitionRef,
                definition.Version,
                definition.ServiceTypeCode,
                definition.ServiceSubCode,
                definition.SubCodeSource,
                definition.GroupCode,
                definition.SubGroupCode,
                definition.Description1Code,
                definition.Description2Code,
                definition.PricingUnit,
                definition.ServiceDateBasis,
                definition.CommercialName,
                definition.Description,
                definition.Document.Type,
                definition.Document.Rfic,
                definition.Document.Rfisc,
                definition.Booking.Method,
                definition.Booking.SsrCode,
                definition.Booking.SsimCode,
                definition.SalesEffectiveFrom,
                definition.SalesDiscontinueOn,
                definition.Status,
                definition.CreatedAt,
                definition.ActivatedAt,
                definition.SuspendedAt,
                definition.RetiredAt);
    }
}
