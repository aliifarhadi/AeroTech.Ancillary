using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class AncillaryServiceDefinitionFormMapper
    {
        public static BackofficeServiceDefinitionFormDto<TSpecification> ToBackofficeForm<TSpecification>(
            AncillaryServiceDefinition definition,
            string supplierName,
            TSpecification specification)
            where TSpecification : class
            => new(
                definition.Id,
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                definition.Version,
                EnumValueDto.Of(definition.Status),
                EnumValueDto.Of(definition.Profile!.Value),
                definition.Variant!.Name,
                supplierName,
                definition.CreatedAt,
                definition.ActivatedAt,
                definition.SuspendedAt,
                definition.RetiredAt,
                new ServiceDefinitionFormDto<TSpecification>(
                    definition.SupplierId,
                    definition.ServiceSubCode,
                    definition.SubCodeSource,
                    definition.ServiceTypeCode,
                    definition.GroupCode,
                    definition.SubGroupCode,
                    definition.Description1Code,
                    definition.Description2Code,
                    definition.PricingUnit!.Value,
                    definition.ServiceDateBasis!.Value,
                    definition.VariantCode!,
                    definition.DocumentRouting!.Value,
                    specification,
                    definition.CommercialName,
                    definition.Description,
                    new ServiceDefinitionDocumentFormDto(
                        definition.Document.Type,
                        definition.Document.Rfic,
                        definition.Document.Rfisc),
                    new ServiceDefinitionBookingFormDto(
                        definition.Booking.Method,
                        definition.Booking.SsrCode,
                        definition.Booking.SsimCode,
                        definition.Booking.ConfirmationRequirement),
                    definition.SalesEffectiveFrom,
                    definition.SalesDiscontinueOn));
    }
}
