using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record PetAnimalDto(
        PetAnimalType AnimalType,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? OtherCode);
}
