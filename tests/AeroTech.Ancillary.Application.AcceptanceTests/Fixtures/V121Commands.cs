using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestChangeProvisionPassengerEligibilityCommand(long ProvisionId, ProvisionPassengerEligibilityInput? PassengerEligibility) : IChangeProvisionPassengerEligibilityCommand;

public sealed record TestChangeProvisionSalesRestrictionsCommand(long ProvisionId, ProvisionSalesRestrictionsInput? SalesRestrictions) : IChangeProvisionSalesRestrictionsCommand;

public sealed record TestChangeProvisionGeographyCommand(long ProvisionId, ProvisionGeographyInput? Geography) : IChangeProvisionGeographyCommand;

public sealed record TestChangeProvisionFlightApplicationCommand(long ProvisionId, ProvisionFlightApplicationInput? FlightApplication) : IChangeProvisionFlightApplicationCommand;

public sealed record TestChangeProvisionFareApplicationCommand(long ProvisionId, ProvisionFareApplicationInput? FareApplication) : IChangeProvisionFareApplicationCommand;

public sealed record TestChangeProvisionTravelDateCommand(long ProvisionId, ProvisionTravelDateInput? TravelDate) : IChangeProvisionTravelDateCommand;

public sealed record TestChangeProvisionDayTimeApplicationCommand(long ProvisionId, ProvisionDayTimeApplicationInput? DayTimeApplication) : IChangeProvisionDayTimeApplicationCommand;

public sealed record TestChangeProvisionAdvancePurchaseCommand(long ProvisionId, ProvisionAdvancePurchaseInput? AdvancePurchase) : IChangeProvisionAdvancePurchaseCommand;

public sealed record TestChangeProvisionBaggageApplicationCommand(long ProvisionId, ProvisionBaggageApplicationInput? BaggageApplication) : IChangeProvisionBaggageApplicationCommand;

public sealed record TestChangeProvisionSeatApplicationCommand(long ProvisionId, ProvisionSeatApplicationInput? SeatApplication) : IChangeProvisionSeatApplicationCommand;

public sealed record TestPermittedTravelPeriodRowCommand(long ProvisionId, long RowId, DateOnly StartDate, DateOnly EndDate)
    : IAddProvisionPermittedTravelPeriodCommand, IChangeProvisionPermittedTravelPeriodCommand, IRemoveProvisionPermittedTravelPeriodCommand;

public sealed record TestBlackoutPeriodRowCommand(long ProvisionId, long RowId, DateOnly StartDate, DateOnly EndDate)
    : IAddProvisionBlackoutPeriodCommand, IChangeProvisionBlackoutPeriodCommand, IRemoveProvisionBlackoutPeriodCommand;

public sealed record TestDayTimeWindowRowCommand(
    long ProvisionId,
    long RowId,
    byte DaysOfWeekMask,
    TimeOnly? StartLocalTime,
    TimeOnly? EndLocalTime,
    DayTimeRestrictionEffect Effect)
    : IAddProvisionDayTimeWindowCommand, IChangeProvisionDayTimeWindowCommand, IRemoveProvisionDayTimeWindowCommand;

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

    public static TestDayTimeWindowRowCommand WindowRow(long provisionId, long rowId, ProvisionDayTimeWindowInput window)
        => new(provisionId, rowId, window.DaysOfWeekMask, window.StartLocalTime, window.EndLocalTime, window.Effect);
}
