using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SelectedUnitInput(int? Quantity = null);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SelectedUnitInput<TOptions>(int? Quantity = null, TOptions? Options = null)
        where TOptions : class;
}
