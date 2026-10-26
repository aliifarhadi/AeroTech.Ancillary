using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AncillaryProductGroupDto<TDetails>(
        string ProductKey,
        string ServiceDefinitionRef,
        string Title,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Description,
        TDetails Details,
        IReadOnlyList<AncillaryUnitOfferDto> Units);
}
