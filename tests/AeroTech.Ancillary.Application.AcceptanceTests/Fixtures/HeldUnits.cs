using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record HeldUnitView(
    long UnitId,
    long OrderServiceId,
    AncillaryReservationUnitStatus Status,
    string? CancellationReasonCode,
    HoldUnitDto? Unit,
    HoldCoverageDto? Coverage);

public static class HeldUnits
{
    public static IReadOnlyList<HeldUnitView> Units(this AncillaryHoldDto hold)
        => Of(hold.Baggages, View)
            .Concat(Of(hold.Meals, View))
            .Concat(Of(hold.AssistedTravel, View))
            .Concat(Of(hold.AirportServices, View))
            .Concat(Of(hold.PriorityServices, View))
            .Concat(Of(hold.Connectivity, View))
            .Concat(Of(hold.Upgrades, View))
            .Concat(Of(hold.Seats, seat => new HeldUnitView(seat.UnitId, seat.OrderServiceId, seat.Status, seat.CancellationReasonCode, seat.Unit, seat.Coverage)))
            .OrderBy(unit => unit.UnitId)
            .ToList();

    private static HeldUnitView View(AncillaryUnitHoldDto unit)
        => new(unit.UnitId, unit.OrderServiceId, unit.Status, unit.CancellationReasonCode, unit.Unit, unit.Coverage);

    private static HeldUnitView View<TOptions>(AncillaryUnitHoldDto<TOptions> unit)
        where TOptions : class
        => new(unit.UnitId, unit.OrderServiceId, unit.Status, unit.CancellationReasonCode, unit.Unit, unit.Coverage);

    private static IEnumerable<HeldUnitView> Of<TDetails, TUnit>(IReadOnlyList<AncillaryVariantHoldDto<TDetails, TUnit>>? variants, Func<TUnit, HeldUnitView> view)
        where TDetails : class
        => (variants ?? []).SelectMany(variant => variant.Products).SelectMany(product => product.Units).Select(view);

    private static IEnumerable<HeldUnitView> Of<TUnit>(IReadOnlyList<AncillaryVariantHoldDto<TUnit>>? variants, Func<TUnit, HeldUnitView> view)
        => (variants ?? []).SelectMany(variant => variant.Products).SelectMany(product => product.Units).Select(view);
}
