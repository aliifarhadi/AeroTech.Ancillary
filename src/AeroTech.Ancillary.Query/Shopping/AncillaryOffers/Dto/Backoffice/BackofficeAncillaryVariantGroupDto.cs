namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryVariantGroupDto<TDetails>(
        string VariantCode,
        string Title,
        IReadOnlyList<BackofficeAncillaryProductGroupDto<TDetails>> Products);
}
