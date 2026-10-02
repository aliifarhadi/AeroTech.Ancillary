using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode
{
    public sealed record ServiceSubCodeResult(
        long Id,
        int OwnerAirlineId,
        string Code,
        ServiceSubCodeSource Source,
        ServiceSubCodeStatus Status);
}
