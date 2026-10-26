using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record AncillaryUnitHoldDto(
        long UnitId,
        long OrderServiceId,
        AncillaryReservationUnitStatus Status,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? CancellationReasonCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] HoldUnitDto? Unit,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] HoldCoverageDto? Coverage,
        HeldSelectedUnitDto Selected);

    public sealed record AncillaryUnitHoldDto<TOptions>(
        long UnitId,
        long OrderServiceId,
        AncillaryReservationUnitStatus Status,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? CancellationReasonCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] HoldUnitDto? Unit,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] HoldCoverageDto? Coverage,
        HeldSelectedUnitDto<TOptions> Selected)
        where TOptions : class;
}
