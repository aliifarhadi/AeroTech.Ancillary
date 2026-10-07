using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public static class AncillaryServiceDefinitionMapper
    {
        public static BackofficeServiceDefinitionDto ToBackofficeServiceDefinition(AncillaryServiceDefinitionReadModel definition)
            => new(
                definition.Id,
                definition.OwnerAirlineId,
                definition.SupplierId,
                definition.SupplierName,
                definition.ServiceDefinitionRef,
                definition.Version,
                definition.ServiceTypeCode,
                definition.ServiceSubCode,
                EnumValueDto.Of(definition.SubCodeSource),
                definition.GroupCode,
                definition.SubGroupCode,
                definition.Description1Code,
                definition.Description2Code,
                definition.CommercialName,
                definition.Description,
                EnumValueDto.Of(definition.DocumentType),
                definition.DocumentRfic,
                definition.DocumentRfisc,
                EnumValueDto.Of(definition.BookingMethod),
                definition.BookingSsrCode,
                definition.BookingSsimCode,
                definition.SalesEffectiveFrom,
                definition.SalesDiscontinueOn,
                EnumValueDto.Of(definition.Status),
                definition.CreatedAt,
                definition.ActivatedAt);
    }
}
