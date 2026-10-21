using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatDefinitionDto(
        long Id,
        string ServiceDefinitionRef,
        int Version,
        long SupplierId,
        string CommercialName,
        string? Description,
        EnumValueDto? Profile,
        string? VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        EnumValueDto SubCodeSource,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        EnumValueDto? PricingUnit,
        EnumValueDto? ServiceDateBasis,
        ServiceSeatBookingDto Booking,
        ServiceSeatDocumentDto Document,
        ServiceSeatSpecificationDto? Seat);
}
