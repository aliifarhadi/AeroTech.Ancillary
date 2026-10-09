using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public static class AncillaryServiceDefinitionMapper
    {
        public static BackofficeServiceDefinitionDto ToBackofficeServiceDefinition(
            AncillaryServiceDefinitionReadModel definition,
            AncillaryServiceDefinition? authored)
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
                EnumValueDto.OfNullable(definition.PricingUnit),
                EnumValueDto.OfNullable(definition.ServiceDateBasis),
                EnumValueDto.OfNullable(definition.Profile),
                definition.VariantCode,
                AncillaryVariant.Find(definition.VariantCode)?.Name,
                EnumValueDto.OfNullable(definition.DocumentRouting),
                authored is { IsClassified: true } ? ServiceSpecificationMapper.ToDto(authored.SpecificationArgs()) : null,
                authored?.SelectionContract is { } contract
                    ? new CustomerSelectionContractDto(
                        EnumValueDto.Of(contract.Kind),
                        contract.ZeroQuantityMeansNoSelection,
                        contract.Fields.Select(field => new CustomerSelectionFieldDto(field.Name, EnumValueDto.Of(field.Type), field.Required)).ToList())
                    : null,
                definition.CommercialName,
                definition.Description,
                EnumValueDto.Of(definition.DocumentType),
                definition.DocumentRfic,
                definition.DocumentRfisc,
                EnumValueDto.Of(definition.BookingMethod),
                definition.BookingSsrCode,
                definition.BookingSsimCode,
                EnumValueDto.Of(definition.BookingConfirmationRequirement),
                definition.SalesEffectiveFrom,
                definition.SalesDiscontinueOn,
                EnumValueDto.Of(definition.Status),
                definition.CreatedAt,
                definition.ActivatedAt,
                definition.SuspendedAt,
                definition.RetiredAt);

        public static ServiceDefinitionPaginatedRowDto ToPaginatedRow(AncillaryServiceDefinitionReadModel definition)
            => new()
            {
                Id = definition.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = definition.OwnerAirlineId,
                ServiceDefinitionRef = definition.ServiceDefinitionRef,
                Version = definition.Version,
                CommercialName = definition.CommercialName,
                ServiceSubCode = definition.ServiceSubCode,
                SubCodeSource = EnumValueDto.Of(definition.SubCodeSource),
                ServiceTypeCode = definition.ServiceTypeCode,
                GroupCode = definition.GroupCode,
                PricingUnit = EnumValueDto.OfNullable(definition.PricingUnit),
                ServiceDateBasis = EnumValueDto.OfNullable(definition.ServiceDateBasis),
                Profile = EnumValueDto.OfNullable(definition.Profile),
                VariantCode = definition.VariantCode,
                SupplierId = definition.SupplierId.ToString(CultureInfo.InvariantCulture),
                SupplierName = definition.SupplierName,
                DocumentType = EnumValueDto.Of(definition.DocumentType),
                Status = EnumValueDto.Of(definition.Status),
                CreatedAt = definition.CreatedAt
            };
    }
}
