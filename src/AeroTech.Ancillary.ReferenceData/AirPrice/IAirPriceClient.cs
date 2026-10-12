using AeroTech.Ancillary.ReferenceData.AirPrice.Wire;

namespace AeroTech.Ancillary.ReferenceData.AirPrice
{
    public interface IAirPriceClient
    {
        Task<List<AirFareDto>> GetAirFaresAsync(CancellationToken cancellationToken = default);

        Task<List<PointOfSaleDto>> GetPointOfSalesAsync(CancellationToken cancellationToken = default);
    }
}
