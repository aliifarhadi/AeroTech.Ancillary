namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record SelectionOptionsDto(string VariantCode, IReadOnlyDictionary<string, SelectionFieldDto> Fields);
}
