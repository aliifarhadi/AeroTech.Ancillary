using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedPricingDto(
        long PricingId,
        long PricingRateId,
        int PricingVersion,
        int CurrencyId,
        int Quantity,
        decimal UnitAmount,
        decimal TotalAmount,
        PricingUnit PricingUnit,
        decimal BaseAmount,
        IReadOnlyList<ServicePriceLineDto> TaxLines,
        IReadOnlyList<ServicePriceLineDto> FeeLines);
}
