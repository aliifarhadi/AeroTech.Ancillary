using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Shopping.SeatMaps
{
    public interface ISeatMapPricingEngine
    {
        Task<SeatMapPricingResult> PriceSeatsAsync(
            AncillaryShoppingContext context,
            ShoppingFilter family,
            IReadOnlyList<SeatMapSeat> seats,
            CancellationToken cancellationToken = default);
    }
}
