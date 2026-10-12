using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record BackofficeServiceDefinitionFormDto<TSpecification>(
        long Id,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        int Version,
        EnumValueDto Status,
        EnumValueDto Profile,
        string VariantName,
        string SupplierName,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        ServiceDefinitionFormDto<TSpecification> Form)
        where TSpecification : class;

    public sealed record ServiceDefinitionFormDto<TSpecification>(
        long SupplierId,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string ServiceTypeCode,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        PricingUnit PricingUnit,
        ServiceDateBasis ServiceDateBasis,
        string VariantCode,
        DocumentRouting DocumentRouting,
        TSpecification Specification,
        string CommercialName,
        string? Description,
        ServiceDefinitionDocumentFormDto Document,
        ServiceDefinitionBookingFormDto Booking,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn)
        where TSpecification : class;

    public sealed record ServiceDefinitionDocumentFormDto(
        AncillaryDocumentType Type,
        string? Rfic,
        string? Rfisc);

    public sealed record ServiceDefinitionBookingFormDto(
        BookingMethod Method,
        string? SsrCode,
        string? SsimCode,
        ConfirmationRequirement ConfirmationRequirement);
}
