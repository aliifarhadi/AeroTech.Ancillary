namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record AncillaryVariantHoldDto<TUnit>(
        string VariantCode,
        IReadOnlyList<AncillaryProductHoldDto<TUnit>> Products);

    public sealed record AncillaryVariantHoldDto<TDetails, TUnit>(
        string VariantCode,
        IReadOnlyList<AncillaryProductHoldDto<TDetails, TUnit>> Products)
        where TDetails : class;
}
