using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts
{
    public sealed record AncillaryServiceDefinitionReadModelSnapshot(
        long ServiceDefinitionId,
        int OwnerAirlineId,
        long SupplierId,
        string SupplierName,
        string ServiceDefinitionRef,
        int Version,
        string ServiceTypeCode,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        string? Description,
        AncillaryDocumentType DocumentType,
        string? DocumentRfic,
        string? DocumentRfisc,
        BookingMethod BookingMethod,
        string? BookingSsrCode,
        string? BookingSsimCode,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn,
        ServiceDefinitionStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt);
}
