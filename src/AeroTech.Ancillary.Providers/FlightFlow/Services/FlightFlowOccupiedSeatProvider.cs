using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.FlightFlow.Wire;

namespace AeroTech.Ancillary.Providers.FlightFlow.Services
{
    public sealed class FlightFlowOccupiedSeatProvider : IOccupiedSeatProvider
    {
        private const string OccupiedSeatsRoute = "v1/Flights/OccupiedSeats";

        private readonly HttpClient _httpClient;

        public FlightFlowOccupiedSeatProvider(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
        {
            if (flightIds.Count == 0)
                return new Dictionary<long, IReadOnlySet<string>>();

            if (_httpClient.BaseAddress is null)
                throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider address is not configured");

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(OccupiedSeatsRoute, new { FlightIds = flightIds }, FlightFlowJson.Options, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved($"status {(int)response.StatusCode}");

                var envelope = await response.Content.ReadFromJsonAsync<FlightFlowEnvelope<List<FlightFlowOccupiedSeats>>>(FlightFlowJson.Options, cancellationToken);

                if (envelope?.Data is not { } flights)
                    throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the answer holds no flights");

                return flights
                    .GroupBy(flight => flight.FlightId)
                    .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlySet<string>)group
                            .SelectMany(flight => flight.Seats ?? [])
                            .Where(seat => !string.IsNullOrWhiteSpace(seat.Seat))
                            .Select(seat => seat.Seat!.Trim())
                            .ToHashSet(StringComparer.OrdinalIgnoreCase));
            }
            catch (HttpRequestException)
            {
                throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider is unreachable");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider did not answer in time");
            }
            catch (JsonException)
            {
                throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the answer is not readable");
            }
        }
    }
}
