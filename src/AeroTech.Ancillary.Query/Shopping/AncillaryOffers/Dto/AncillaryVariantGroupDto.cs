namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AncillaryVariantGroupDto<TDetails>(
        string VariantCode,
        string Title,
        IReadOnlyList<AncillaryProductGroupDto<TDetails>> Products);
}
