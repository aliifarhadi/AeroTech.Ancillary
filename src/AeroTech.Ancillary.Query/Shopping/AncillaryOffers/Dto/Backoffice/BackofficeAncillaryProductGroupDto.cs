using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryProductGroupDto<TDetails>(
        string ProductKey,
        string ServiceDefinitionRef,
        string Title,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Description,
        TDetails Details,
        IReadOnlyList<BackofficeAncillaryUnitOfferDto> Units);
}
