using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.Offer.Wire;

namespace AeroTech.Ancillary.Providers.Offer.Services
{
    public sealed class FlightOfferProvider : IFlightOfferProvider
    {
        private const string OfferDetailRoute = "v1/FlightOffers/Details";

        private readonly HttpClient _httpClient;

        public FlightOfferProvider(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<FlightOffer> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default)
        {
            if (_httpClient.BaseAddress is null)
                throw ExceptionFactory.FlightOfferCouldNotBeRetrieved("the provider address is not configured");

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(OfferDetailRoute, new { OfferId = offerId }, OfferJson.Options, cancellationToken);
                var envelope = await ReadAsync(response, cancellationToken);

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
                    throw ExceptionFactory.FlightOfferWasRefused(FirstError(envelope) ?? Status(response));

                if (!response.IsSuccessStatusCode || envelope?.Data is null)
                    throw ExceptionFactory.FlightOfferCouldNotBeRetrieved(Status(response));

                return FlightOfferResponseMapper.ToDomain(envelope.Data, offerId);
            }
            catch (HttpRequestException)
            {
                throw ExceptionFactory.FlightOfferCouldNotBeRetrieved("the provider is unreachable");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw ExceptionFactory.FlightOfferCouldNotBeRetrieved("the provider did not answer in time");
            }
        }

        private static async Task<OfferEnvelope<FlightOfferDetailResponse>?> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<OfferEnvelope<FlightOfferDetailResponse>>(OfferJson.Options, cancellationToken);
            }
            catch (JsonException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private static string Status(HttpResponseMessage response) => $"HTTP {(int)response.StatusCode}";

        private static string? FirstError(OfferEnvelope<FlightOfferDetailResponse>? envelope)
        {
            var error = envelope?.Errors?.FirstOrDefault();

            return error is null ? null : error.Detail ?? error.Title;
        }
    }
}
