using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record BackofficeServiceDefinitionDto(
        long Id,
        int OwnerAirlineId,
        long SupplierId,
        string SupplierName,
        string ServiceDefinitionRef,
        int Version,
        string ServiceTypeCode,
        string ServiceSubCode,
        EnumValueDto SubCodeSource,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        string? Description,
        EnumValueDto DocumentType,
        string? DocumentRfic,
        string? DocumentRfisc,
        EnumValueDto BookingMethod,
        string? BookingSsrCode,
        string? BookingSsimCode,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn,
        EnumValueDto Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt);
}
