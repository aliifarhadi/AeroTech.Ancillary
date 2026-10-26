using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record SelectedUnitDto(
        int Quantity,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SelectionOptionsDto<JsonObject>? Options);
}
