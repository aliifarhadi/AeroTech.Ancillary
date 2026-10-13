using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.ReferenceData.AirPrice.Wire;

namespace AeroTech.Ancillary.ReferenceData.AirPrice
{
    public sealed class AirPriceClient : IAirPriceClient
    {
        private const string AirFaresRoute = "v1";
        private const string PointOfSalesRoute = "v1/PointOfSales";
        private const string RateOfExchangesRoute = "v1/RateOfExchangeTables";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _httpClient;

        public AirPriceClient(HttpClient httpClient) => _httpClient = httpClient;

        public Task<List<AirFareDto>> GetAirFaresAsync(CancellationToken cancellationToken = default)
            => GetAsync<AirFareDto>(AirFaresRoute, cancellationToken);

        public Task<List<PointOfSaleDto>> GetPointOfSalesAsync(CancellationToken cancellationToken = default)
            => GetAsync<PointOfSaleDto>(PointOfSalesRoute, cancellationToken);

        public Task<List<RateOfExchangeDto>> GetRateOfExchangesAsync(CancellationToken cancellationToken = default)
            => GetAsync<RateOfExchangeDto>(RateOfExchangesRoute, cancellationToken);

        private async Task<List<T>> GetAsync<T>(string route, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(route, cancellationToken);
            var envelope = await response.Content.ReadFromJsonAsync<AirPriceEnvelope<List<T>>>(JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode || envelope?.Data is null)
                throw new AirPriceRequestException(FirstError(envelope) ?? $"AirPrice request '{route}' failed with status {(int)response.StatusCode}.");

            return envelope.Data;
        }

        private static string? FirstError<T>(AirPriceEnvelope<T>? envelope)
        {
            var error = envelope?.Errors?.FirstOrDefault();
            return error is null ? null : error.Detail ?? error.Title;
        }
    }
}
