using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldSelectedUnitDto(int Quantity);

    public sealed record HeldSelectedUnitDto<TOptions>(
        int Quantity,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TOptions? Options)
        where TOptions : class;
}
