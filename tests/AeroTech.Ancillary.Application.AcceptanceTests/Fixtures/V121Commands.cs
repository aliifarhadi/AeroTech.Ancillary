using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestAssignServiceDateBasisCommand(long ServiceDefinitionId, ServiceDateBasis ServiceDateBasis)
    : IAssignAncillaryServiceDefinitionServiceDateBasisCommand;

public static class V121Commands
{
    public const byte Monday = 1;
    public const byte Tuesday = 2;
    public const byte Wednesday = 4;
    public const byte Thursday = 8;
    public const byte Friday = 16;
    public const byte Saturday = 32;
    public const byte Sunday = 64;
    public const byte Weekdays = 31;
    public const byte EveryDay = 127;

    public static DateOnly Day(int year, int month, int day) => new(year, month, day);

    public static ProvisionDatePeriodInput Period(DateOnly startDate, DateOnly endDate) => new(startDate, endDate);

    public static ProvisionTravelDateInput Dates(ProvisionDatePeriodInput[]? permitted = null, ProvisionDatePeriodInput[]? blackout = null) => new(permitted, blackout);

    public static ProvisionDayTimeWindowInput Window(
        byte daysOfWeekMask,
        int? fromHour = null,
        int? toHour = null,
        DayTimeRestrictionEffect effect = DayTimeRestrictionEffect.Allow)
        => new(
            daysOfWeekMask,
            fromHour is null ? null : new TimeOnly(fromHour.Value, 0),
            toHour is null ? null : new TimeOnly(toHour.Value, 0),
            effect);

    public static ProvisionDayTimeApplicationInput DayTime(params ProvisionDayTimeWindowInput[] windows) => new(windows);

    public static ProvisionServiceLocationInput Location(ServiceLocationType locationType, int locationId) => new(locationType, locationId);
}
