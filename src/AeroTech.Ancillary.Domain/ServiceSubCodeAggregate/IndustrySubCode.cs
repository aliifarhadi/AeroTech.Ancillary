using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate
{
    public sealed record IndustrySubCode(
        string Code,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        string Rfic,
        AncillaryDocumentType DocumentType,
        int QuantityPerOccurrence);
}
