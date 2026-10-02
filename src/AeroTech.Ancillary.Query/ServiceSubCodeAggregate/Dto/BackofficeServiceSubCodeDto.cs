using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto
{
    public sealed record BackofficeServiceSubCodeDto(
        long Id,
        int OwnerAirlineId,
        string Code,
        EnumValueDto Source,
        string Rfic,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        EnumValueDto Status,
        DateTimeOffset CreatedAt);
}
