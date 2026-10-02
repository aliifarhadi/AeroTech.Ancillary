using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts
{
    public sealed record ServiceSubCodeReadModelSnapshot(
        long ServiceSubCodeId,
        int OwnerAirlineId,
        string Code,
        ServiceSubCodeSource Source,
        string Rfic,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        ServiceSubCodeStatus Status,
        DateTimeOffset CreatedAt);
}
