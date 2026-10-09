using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.FlightFlow.Wire;

namespace AeroTech.Ancillary.Providers.FlightFlow.Services
{
    public sealed class FlightFlowFlightOccurrenceReference : IFlightOccurrenceReference
    {
        private const string FlightsRoute = "v1/Flights";

        private readonly HttpClient _httpClient;

        public FlightFlowFlightOccurrenceReference(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken = default)
        {
            if (_httpClient.BaseAddress is null)
                return InventoryReferenceCheck.SourceUnavailable;

            try
            {
                using var response = await _httpClient.GetAsync($"{FlightsRoute}/{flightId}", cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return InventoryReferenceCheck.NotFound;

                if (!response.IsSuccessStatusCode)
                    return InventoryReferenceCheck.SourceUnavailable;

                var envelope = await response.Content.ReadFromJsonAsync<FlightFlowEnvelope<FlightFlowFlight>>(FlightFlowJson.Options, cancellationToken);

                if (envelope?.Data is not { } flight || flight.Id != flightId)
                    return InventoryReferenceCheck.SourceUnavailable;

                return flight.OperatingAirlineId == ownerAirlineId
                    ? InventoryReferenceCheck.Verified
                    : InventoryReferenceCheck.NotFound;
            }
            catch (HttpRequestException)
            {
                return InventoryReferenceCheck.SourceUnavailable;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return InventoryReferenceCheck.SourceUnavailable;
            }
            catch (JsonException)
            {
                return InventoryReferenceCheck.SourceUnavailable;
            }
        }
    }
}
