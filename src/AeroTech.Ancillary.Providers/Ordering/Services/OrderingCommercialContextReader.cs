using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.Ordering.Wire;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Providers.Ordering.Services
{
    public sealed class OrderingCommercialContextReader : IOrderCommercialContextReader
    {
        private const string AncillaryContextRoute = "v1/Orders/{0}/AncillaryContext";

        private readonly HttpClient _httpClient;
        private readonly IClock _clock;

        public OrderingCommercialContextReader(HttpClient httpClient, IClock clock)
        {
            _httpClient = httpClient;
            _clock = clock;
        }

        public async Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default)
        {
            if (_httpClient.BaseAddress is null)
                throw ExceptionFactory.OrderSourceUnavailable(orderId, "the provider address is not configured");

            try
            {
                using var response = await _httpClient.GetAsync(string.Format(CultureInfo.InvariantCulture, AncillaryContextRoute, orderId), cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                    throw ExceptionFactory.OrderSourceUnavailable(orderId, $"status {(int)response.StatusCode}");

                var envelope = await response.Content.ReadFromJsonAsync<OrderingEnvelope<OrderingAncillaryContext>>(OrderingJson.Options, cancellationToken);

                return envelope?.Data is { } context
                    ? OrderingAncillaryContextMapper.ToDomain(context, orderId, _clock.GetDateTime())
                    : throw ExceptionFactory.OrderSourceUnavailable(orderId, "the answer holds no order context");
            }
            catch (HttpRequestException)
            {
                throw ExceptionFactory.OrderSourceUnavailable(orderId, "the provider is unreachable");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw ExceptionFactory.OrderSourceUnavailable(orderId, "the provider did not answer in time");
            }
            catch (JsonException)
            {
                throw ExceptionFactory.OrderSourceUnavailable(orderId, "the answer is not readable");
            }
        }
    }
}
